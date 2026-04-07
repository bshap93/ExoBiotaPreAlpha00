using System;
using System.Collections;
using Animancer;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using Helpers.ScriptableObjects.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    public class AlienNPCAnimancerController : MonoBehaviour
    {
        [SerializeField] bool hasWorkingAnimations;
        [SerializeField] bool hasIdleAnimations;
        [SerializeField] AlienNPCState initialState;

        [Header("Animations")] [ShowIf("hasWorkingAnimations")] [SerializeField]
        EnemyToolWeaponAnimationSet.ActionClip[] alienWorkingAnimations;
        [ShowIf("hasIdleAnimations")] [SerializeField]
        EnemyToolWeaponAnimationSet.ActionClip[] alienIdleAnimations;

        [SerializeField] AnimancerComponent animancerComponent;

        [Header("Audio")] [SerializeField] AudioSource audioSource;

        [SerializeField] CreatureController creatureController;

        [SerializeField] AvatarMask upperBodyMask;
        Coroutine _pendingAudioCoroutine;

        AnimancerLayer _upperBodyLayer;

        public bool IsWorking => CurrentState == AlienNPCState.Working;

        public AlienNPCState CurrentState { get; private set; }

        void Start()
        {
            PlayAnimationsForState(initialState);
        }

        public void PlayAnimationsForState(AlienNPCState state)
        {
            CurrentState = state;
            switch (state)
            {
                case AlienNPCState.Working:
                    PlaySequenceOfWorkingAnimations();
                    break;
                case AlienNPCState.Idling:
                    PlaySequenceOfIdleAnimations();
                    break;
            }
        }

        public void PlaySequenceOfIdleAnimations()
        {
            if (alienIdleAnimations == null || alienIdleAnimations.Length == 0)
                throw new InvalidOperationException($"[{name}] No idle animations assigned.");

            CancelPendingAudio();

            for (var i = 0; i < alienIdleAnimations.Length; i++)
            {
                var state = animancerComponent.States.GetOrCreate(alienIdleAnimations[i].animationClip);
                // state.Events(this).Clear();
                var capturedNext = alienIdleAnimations[(i + 1) % alienIdleAnimations.Length];
                state.Events(this).OnEnd = () =>
                {
                    // animancerComponent.Play(capturedNext.animationClip);
                    var next = animancerComponent.Play(capturedNext.animationClip);
                    next.Time = 0f; // forces restart even when capturedNext == current clip
                };
            }

            animancerComponent.Play(alienIdleAnimations[0].animationClip);
        }

        void PlaySequenceOfWorkingAnimations()
        {
            if (alienWorkingAnimations == null || alienWorkingAnimations.Length == 0)
            {
                Debug.LogWarning($"[{name}] No working animations assigned.");
                return;
            }

            CancelPendingAudio();

            // Animation chaining: OnEnd only drives the next clip, no audio scheduling here.
            for (var i = 0; i < alienWorkingAnimations.Length; i++)
            {
                var clip = alienWorkingAnimations[i];
                var state = animancerComponent.States.GetOrCreate(clip.animationClip);
                // state.Events(this).Clear();
                var capturedNext = alienWorkingAnimations[(i + 1) % alienWorkingAnimations.Length];
                state.Events(this).OnEnd = () =>
                {
                    // animancerComponent.Play(capturedNext.animationClip);
                    var next = animancerComponent.Play(capturedNext.animationClip);
                    next.Time = 0f; // forces restart even when capturedNext == current clip 
                };
            }

            // animancerComponent.Play(alienWorkingAnimations[0].animationClip);

            // Audio runs in its own loop, timed by each clip's duration.
            // This is decoupled from OnEnd so it can never silently drop a trigger.
            _pendingAudioCoroutine = StartCoroutine(WorkingSequenceWithOffset());
        }

        IEnumerator WorkingSequenceWithOffset()
        {
            // Random offset desynchronizes multiple enemies sharing the same clip set.
            // Range upper bound: total cycle length so the offset is always < one full loop.
            var cycleDuration = 0f;
            foreach (var clip in alienWorkingAnimations)
                cycleDuration += clip.Duration;

            var randomOffset = Random.Range(0f, cycleDuration);
            yield return new WaitForSeconds(randomOffset);

            animancerComponent.Play(alienWorkingAnimations[0].animationClip);
            yield return StartCoroutine(WorkingAudioLoop());
        }

        /// <summary>
        ///     Mirrors the working animation sequence independently.
        ///     Waits audioDelay into each clip, fires a random sound, then waits
        ///     out the remainder of that clip's duration before moving to the next.
        /// </summary>
        IEnumerator WorkingAudioLoop()
        {
            var index = 0;
            while (IsWorking)
            {
                var clip = alienWorkingAnimations[index];

                if (audioSource != null && clip.audioClipList != null && clip.audioClipList.Length > 0)
                {
                    // Play feedbacks before the delay. Delay feedbacks from MMFPlayer Inspector
                    // component if delay is neeeded.
                    clip.feedbacks?.PlayFeedbacks();

                    if (clip.audioDelay > 0f)
                        yield return new WaitForSeconds(clip.audioDelay);

                    audioSource.PlayOneShot(clip.audioClipList[Random.Range(0, clip.audioClipList.Length)]);


                    // Wait out the rest of the clip before moving on.
                    var remaining = clip.Duration - clip.audioDelay;
                    if (remaining > 0f)
                        yield return new WaitForSeconds(remaining);
                }
                else
                {
                    // No audio for this clip — just wait its full duration.
                    if (clip.Duration > 0f)
                        yield return new WaitForSeconds(clip.Duration);
                    else
                        yield return null; // safety: never spin forever on a 0-length clip
                }

                index = (index + 1) % alienWorkingAnimations.Length;
            }
        }

        void CancelPendingAudio()
        {
            if (_pendingAudioCoroutine == null) return;
            StopCoroutine(_pendingAudioCoroutine);
            _pendingAudioCoroutine = null;
        }

        public void PlayWeaponHoldPose(AnimationClip holdPoseClip)
        {
            if (_upperBodyLayer == null) SetupUpperBodyLayer();
            if (_upperBodyLayer != null)
            {
                var state = _upperBodyLayer.Play(holdPoseClip, 0.1f);
                state.Events(this).OnEnd = () => state.Time = state.Duration;
            }
        }

        public void PlayAttackClip(EnemyToolWeaponAnimationSet.ActionClip clip, Action onHit, Action onComplete)
        {
            creatureController.IsPlayingCustomAnimation = true;
            var state = animancerComponent.Layers[0].Play(clip.animationClip, 0.15f);
            clip.feedbacks?.PlayFeedbacks();
            StartCoroutine(
                AttackClipRoutine(
                    clip, state, onHit, () =>
                    {
                        creatureController.IsPlayingCustomAnimation = false;
                        onComplete?.Invoke();
                    }));
        }

        IEnumerator AttackClipRoutine(EnemyToolWeaponAnimationSet.ActionClip clip, AnimancerState state, Action onHit,
            Action onComplete)
        {
            state.Events(this).OnEnd = () => onComplete?.Invoke();

            if (clip.audioDelay > 0f)
                yield return new WaitForSeconds(clip.audioDelay);

            if (clip.audioClipList?.Length > 0)
                audioSource.PlayOneShot(clip.audioClipList[Random.Range(0, clip.audioClipList.Length)]);

            onHit?.Invoke(); // damage/hitbox window opens here, timed to the swing impact
        }

        public void ClearUpperBody()
        {
            _upperBodyLayer?.StartFade(0f, 0.2f);
        }

        void SetupUpperBodyLayer()
        {
            _upperBodyLayer = animancerComponent.Layers[1];
            _upperBodyLayer.Mask = upperBodyMask;
            _upperBodyLayer.Weight = 1f;
        }
    }
}
