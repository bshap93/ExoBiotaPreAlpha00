using System;
using Animancer;
using UnityEngine;

namespace Helpers.AnimancerHelper
{
    namespace Helpers.AnimancerHelper
    {
        /// <summary>
        ///     Sits on the leftArm GameObject alongside its own AnimancerComponent.
        ///     Completely isolated from the right-arm / melee animation system.
        ///     Drive locomotion externally (PlayerEquippedAbility calls SyncLocomotion),
        ///     and call PlayCast() when an ability fires.
        /// </summary>
        public class LeftArmController : MonoBehaviour
        {
            [Header("Animancer")] public AnimancerComponent animancer;

            [Header("Locomotion Clips")] public AnimationClip idleClip;
            public AnimationClip walkClip;
            public AnimationClip runClip; // optional — falls back to walk if null

            [Header("Transition Speeds")] [SerializeField]
            float locoTransitionDuration = 0.15f;
            [SerializeField] float castTransitionDuration = 0.1f;
            LocoMode _currentLoco = LocoMode.Idle;
            bool _isCasting;

            // ── runtime ──────────────────────────────────────────────────────────
            AnimancerState _locoState;

            void OnValidate()
            {
                if (animancer == null) animancer = GetComponent<AnimancerComponent>();
            }

            // ── public API ───────────────────────────────────────────────────────

            /// <summary>Call this on ability equip so the arm starts looping idle.</summary>
            public void StartIdle()
            {
                _isCasting = false;
                PlayLoco(LocoMode.Idle);
            }

            /// <summary>
            ///     Mirror whatever locomotion mode the right arm is in.
            ///     Call from PlayerEquippedAbility.Update or wherever loco is driven.
            /// </summary>
            public void SyncLocomotion(bool isWalking, bool isRunning)
            {
                if (_isCasting) return;

                var target = isRunning ? LocoMode.Run
                    : isWalking ? LocoMode.Walk
                    : LocoMode.Idle;

                if (target != _currentLoco) PlayLoco(target);
            }

            /// <summary>
            ///     Play the cast clip once, then return to locomotion.
            ///     Typically called right after CurrentRuntimeAbility.Use().
            /// </summary>
            public void PlayCast(AnimationClip castClip, Action onComplete = null)
            {
                if (castClip == null || animancer == null) return;

                _isCasting = true;
                var state = animancer.Play(castClip, castTransitionDuration);
                state.Time = 0f;
                state.Speed = 1f;
                state.Events(this).Clear();

                state.Events(this).OnEnd = () =>
                {
                    _isCasting = false;
                    ReturnToLoco();
                    onComplete?.Invoke();
                };
            }

            // ── private helpers ──────────────────────────────────────────────────

            void PlayLoco(LocoMode mode)
            {
                var clip = PickClip(mode);
                if (clip == null) return;

                _currentLoco = mode;
                _locoState = animancer.Play(clip, locoTransitionDuration);
                _locoState.Events(this).OnEnd = () => { _locoState.Time = 0f; }; // loop
            }

            void ReturnToLoco()
            {
                PlayLoco(_currentLoco);
            }

            AnimationClip PickClip(LocoMode mode)
            {
                return mode switch
                {
                    LocoMode.Run => runClip != null ? runClip : walkClip,
                    LocoMode.Walk => walkClip != null ? walkClip : idleClip,
                    _ => idleClip
                };
            }

            enum LocoMode
            {
                Idle,
                Walk,
                Run
            }
        }
    }
}
