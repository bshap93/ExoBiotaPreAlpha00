using System;
using System.Collections;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.InputHandling;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using Helpers.Events.ManagerEvents;
using Helpers.ScriptableObjects.Animation;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public abstract class MeleeToolPrefab : MonoBehaviour, IRuntimeTool
    {
        [SerializeField] protected float pullbackQuicknessFactor = 1f;

        [FormerlySerializedAs("ToolAttackProfile")]
        public PlayerToolAttackProfile toolAttackProfile;

        [SerializeField] protected AttributesManager attributesManager;
        [SerializeField] protected float agilityCooldownSecondsReducePerPoint;

        [SerializeField] CanBeAreaScannedType detectableType = CanBeAreaScannedType.BasicBioScanner;

        [SerializeField] RightHandEquippableTool rightHandEquippableTool;

        [Header("Swing Animation Settings")] [Tooltip("Use multiple swing animations that alternate?")]
        public bool useMultipleSwings = true;

        [Tooltip("Delay in seconds before hit is applied for Swing 01 animation.")]
        public float swing01HitDelay = 0.2f;

        [Tooltip("Delay in seconds before hit is applied for Swing 02 animation.")]
        public float swing02HitDelay = 0.55f;

        [Tooltip("Delay in seconds before hit is applied for Swing 03 animation (if used).")]
        public float swing03HitDelay = 0.22f;

        public float swingHeavyHitDelay = 0.1f;

        [FormerlySerializedAs("swingSpeedMultiplier")] [SerializeField]
        protected float overallToolSwingSpeedMultiplier = 1f;

        [Tooltip("Fallback delay if using beginUseAnimation (legacy mode).")]
        public float defaultHitDelay = 0.2f;

        // public float swingDownHitDelay = 0.1f;

        public bool useOnRelease;

        // [SerializeField] protected float agilityReductionFactor = 0.05f;

        public GameObject ineffectualDebrisEffectPrefab;

        [FormerlySerializedAs("reticleForInabilityToHit")]
        [FormerlySerializedAs("reticleForInabilityToSample")]
        [SerializeField]
        protected Sprite reticleForHittable;

        [SerializeField] protected string[] tagsWhichShouldShowInabilityReticle;

        public MMFeedbacks equipFeedbacks;
        public MMFeedbacks unequippedFeedbacks;

        public MMFeedbacks hitRockFeedbacks;
        public MMFeedbacks hitRigidOrganismFeedbacks;
        public MMFeedbacks hitFleshyFeedbacks;

        [Header("References")] public Camera mainCamera;

        [Tooltip("How far the tool can reach.")]
        public float reach = 3.25f;

        [Tooltip("Optional physics mask to limit what raycast can hit.")]
        public LayerMask hitMask = ~0;

        [ShowIf("useOnRelease")] public float timeToFullCharge = 1.5f;

        [SerializeField] protected GameObject trailRendererGo;


        protected AnimancerArmController AnimController;

        protected int CurrentSwingIndex; // Track which swing we're on
        protected RaycastHit LastHit;


        protected RaycastHit? SavedAimHitInfo;

        void Awake()
        {
            if (trailRendererGo != null) trailRendererGo.SetActive(false);
        }

        public abstract void Initialize(PlayerEquipment owner);

        public abstract void Use();


        public virtual void Unequip()
        {
        }
        public void Equip()
        {
        }

        public virtual bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return true;
        }

        public abstract Sprite GetReticleForTool(GameObject colliderGameObject);
        public bool ToolIsUsedOnRelease()
        {
            return useOnRelease;
        }
        public bool ToolMustBeHeldToUse()
        {
            return false;
        }

        public bool CanAbortAction()
        {
            throw new NotImplementedException();
        }

        public abstract MMFeedbacks GetEquipFeedbacks();
        public abstract MMFeedbacks GetUnequipFeedbacks();

        public CanBeAreaScannedType GetDetectableType()
        {
            return detectableType;
        }

        public virtual void ChargeUse(bool justPressed)
        {
            // Only start animation on initial press
            // if (justPressed) StartChargePullbackAnimation();
            //
            // ChargeTimeElapsed += Time.deltaTime;
            //
            // ChargeToolEvent.Trigger(
            //     ChargeToolEventType.Update,
            //     ChargeTimeElapsed / timeToFullCharge);
        }

        protected void CaptureAim()
        {
            Physics.Raycast(
                mainCamera.transform.position, mainCamera.transform.forward,
                out var hit, reach, hitMask, QueryTriggerInteraction.Ignore);

            SavedAimHitInfo = hit;
        }

        protected IEnumerator ApplyAttackLunge(PlayerAttack attack, float delay)
        {
            if (attack == null || !attack.playerMovesWithAttack) yield break;

            yield return new WaitForSeconds(delay * 0.3f);

            // Cache this reference in Initialize() instead of finding it each time
            var movement = FindFirstObjectByType<MyNormalMovement>();
            if (movement == null) yield break;

            var forward = mainCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            var lungeDuration = 0.15f;
            var elapsed = 0f;
            var lungeSpeed = attack.movementAmount;

            while (elapsed < lungeDuration)
            {
                var t = elapsed / lungeDuration;
                var factor = 1f - t * t;

                movement.SetAttackLungeVelocity(forward * (lungeSpeed * factor));

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Clear it when done
            movement.SetAttackLungeVelocity(Vector3.zero);
        }
        public SecondaryActionType GetSecondaryActionType()
        {
            return SecondaryActionType.BlockWithMeleeWeapon;
        }

        /// <summary>
        ///     Spawns VFX at hit point with automatic cleanup
        /// </summary>
        protected void SpawnHitVFX(GameObject vfxPrefab, Vector3 position, Vector3 normal, float lifetime = 2f)
        {
            if (vfxPrefab == null) return;

            var vfxInstance = Instantiate(vfxPrefab, position, Quaternion.LookRotation(normal));
            Destroy(vfxInstance, lifetime);
        }


        protected PlayerAttack DetermineCorrectPlayerToolAttack(HitType attackType)
        {
            if (attackType == HitType.Normal)
            {
                var basicAttack = toolAttackProfile.basicAttack;
                return basicAttack;
            }

            if (attackType == HitType.Heavy) return toolAttackProfile.heavyAttack;

            return null;
        }


        protected void SpawnFxForIneffectualHit(Vector3 pos, Vector3 normal)
        {
            if (ineffectualDebrisEffectPrefab)
            {
                var debris = Instantiate(
                    ineffectualDebrisEffectPrefab, pos + normal * 0.05f,
                    Quaternion.LookRotation(-mainCamera.transform.forward));

                Destroy(debris, 2f);
            }
        }

        public virtual void PlayHeavySwingSequence()
        {
            var animSet = AnimController.currentToolAnimationSet;
            var swingClip = animSet.heavySwingAnimation;
            var swingSound = animSet.heavySwingAudioClip;
            var speedMultHeavy = animSet.heavySwingSpeedMult;

            var hitDelay = swingHeavyHitDelay / overallToolSwingSpeedMultiplier;

            if (swingClip == null)
            {
                swingClip = animSet.heavySwingAnimation;
                hitDelay = swingHeavyHitDelay;
            }

            if (swingSound != null) StartCoroutine(PlaySoundAfterDelay(swingSound, hitDelay / 2f));

            if (swingClip != null)
            {
                PlaySwingAnimation(swingClip, animSet.heavySwingDurationForTrailRenderer, speedMultHeavy);

                StartCoroutine(ApplyHeavyHitAfterDelay(hitDelay));
            }
            else
            {
                AnimController.PlayToolUseOneShot();
                StartCoroutine(ApplyHeavyHitAfterDelay(defaultHitDelay / overallToolSwingSpeedMultiplier));
            }
        }

        public virtual void PlaySwingSequence()
        {
            var animSet = AnimController.currentToolAnimationSet;
            AnimationClip swingClip = null;
            AudioClip swingSound = null;
            float durationForTrailRenderer = 0;
            var hitDelay = defaultHitDelay / overallToolSwingSpeedMultiplier;
            var speedMult = 1.25f;

            // Determine which swing to use based on current index
            switch (CurrentSwingIndex)
            {
                case 0:
                    swingClip = animSet.swing01Animation;
                    hitDelay = swing01HitDelay / overallToolSwingSpeedMultiplier;
                    swingSound = animSet.swing01AudioClip;
                    speedMult = animSet.swing01SpeedMult;
                    durationForTrailRenderer = animSet.swing01DurationForTrailRenderer;
                    break;
                case 1:
                    swingClip = animSet.swing02Animation;
                    hitDelay = swing02HitDelay / overallToolSwingSpeedMultiplier;
                    swingSound = animSet.swing02AudioClip;
                    speedMult = animSet.swing02SpeedMult;
                    durationForTrailRenderer = animSet.swing02DurationForTrailRenderer;
                    break;
                case 2:
                    swingClip = animSet.swing03Animation;
                    hitDelay = swing03HitDelay / overallToolSwingSpeedMultiplier;
                    swingSound = animSet.swing03AudioClip;
                    speedMult = animSet.swing03SpeedMult;
                    durationForTrailRenderer = animSet.swing03DurationForTrailRenderer;
                    break;
            }

            // If the selected swing doesn't exist, fall back to swing01
            if (swingClip == null)
            {
                swingClip = animSet.swing01Animation;
                hitDelay = swing01HitDelay;
                CurrentSwingIndex = 0;
            }

            if (swingSound != null) StartCoroutine(PlaySoundAfterDelay(swingSound, hitDelay / 2f));
            // AudioSource.PlayClipAtPoint(swingSound, mainCamera.transform.position);

            if (swingClip != null)
            {
                // Play the specific swing animation
                PlaySwingAnimation(swingClip, durationForTrailRenderer, speedMult);

                // Start coroutine with the appropriate delay
                StartCoroutine(ApplyNormalHitAfterDelay(hitDelay));

                // Advance to next swing (with wrap-around)
                AdvanceSwingIndex(animSet);
            }
            else
            {
                // No swing animations available, use legacy mode
                AnimController.PlayToolUseOneShot();
                StartCoroutine(ApplyNormalHitAfterDelay(defaultHitDelay / overallToolSwingSpeedMultiplier));
            }
        }


        protected IEnumerator PlaySoundAfterDelay(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(clip, mainCamera.transform.position);
        }

        IEnumerator SequenceTrailRendererEnable(float duration)
        {
            yield return new WaitForSeconds(duration * 0.4f);

            trailRendererGo.SetActive(true);

            yield return new WaitForSeconds(duration);

            trailRendererGo.SetActive(false);
        }
        public void PlaySwingAnimation(AnimationClip clip, float durationForTrailRenderer, float speedMultiplier = 1f)
        {
            if (AnimController.animancerComponent == null) return;

            var layer = AnimController.animancerComponent.Layers[1];

            // Play swing on Layer 1
            var state = layer.Play(clip, AnimController.defaultTransitionDuration);
            state.NormalizedTime = 0f; // Start from the beginning
            if (trailRendererGo != null) StartCoroutine(SequenceTrailRendererEnable(durationForTrailRenderer));
            //StartCoroutine(SequenceTrailRendererEnable(durationForTrailRenderer));
            state.Speed = speedMultiplier * overallToolSwingSpeedMultiplier;
            layer.Weight = 1f;

            // Mark this as the active action animation
            AnimController.SetActionState(state);

            // Clear previous events to avoid stacked callbacks
            state.Events(this).Clear();


            // Add a SINGLE end event
            state.Events(this).OnEnd = () =>
            {
                // Disable swing layer
                layer.Weight = 0f;

                // if (trailRendererGo != null) trailRendererGo.SetActive(false);


                // Clear action state so locomotion can resume
                AnimController.ClearActionState();

                // Return to locomotion safely
                AnimController.ReturnToLocomotion();
            };
        }

        public void ReturnToLocomotionImmediately()
        {
            if (AnimController.animancerComponent == null) return;

            var layer = AnimController.animancerComponent.Layers[1];
            layer.Weight = 0f;

            // Clear action state so locomotion can resume
            AnimController.ClearActionState();

            // Return to locomotion safely
            AnimController.ReturnToLocomotion();
        }

        public void StartChargePullbackAnimation()
        {
            if (AnimController.animancerComponent == null) return;

            var layer = AnimController.animancerComponent.Layers[1];

            var state = layer.Play(AnimController.currentToolAnimationSet.beginUseAnimation);
            state.Speed *= pullbackQuicknessFactor;
            layer.Weight = 1f;

            AnimController.SetActionState(state);

            state.Events(this).Clear();

            state.Events(this).OnEnd = () =>
            {
                // ToolIsHeldInChargePosition = true;
                // ChargeToolEvent.Trigger(ChargeToolEventType.Start, ChargeTimeElapsed / timeToFullCharge);
                // layer.Weight = 0f;
                // AnimController.ClearActionState();
                // AnimController.ReturnToLocomotion();
            };
        }


        protected virtual void AdvanceSwingIndex(ToolAnimationSet animSet)
        {
            // Count how many swing animations are available
            var availableSwings = 0;
            if (animSet.swing01Animation != null) availableSwings = 1;
            if (animSet.swing02Animation != null) availableSwings = 2;
            if (animSet.swing03Animation != null) availableSwings = 3;

            // Advance and wrap around
            CurrentSwingIndex = (CurrentSwingIndex + 1) % Mathf.Max(1, availableSwings);
        }

        protected virtual IEnumerator ApplyNormalHitAfterDelay(float delay)
        {
            // Wait for the specified delay to sync with animation
            yield return new WaitForSeconds(delay);

            // Perform the actual hit detection and application
            ApplyHit();

            SavedAimHitInfo = null;
        }


        protected virtual IEnumerator ApplyHeavyHitAfterDelay(float delay)
        {
            // Wait for the specified delay to sync with animation
            yield return new WaitForSeconds(delay);

            // Perform the actual hit detection and application
            ApplyHit(HitType.Heavy);

            SavedAimHitInfo = null;
        }

        public abstract void ApplyHit(HitType hitType = HitType.Normal);

        public abstract void PerformToolAction();


        public abstract void PerformHeavyChargedToolAction();
    }
}
