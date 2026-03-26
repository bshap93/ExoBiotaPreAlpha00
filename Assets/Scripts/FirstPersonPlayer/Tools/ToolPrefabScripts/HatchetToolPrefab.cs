using Feedbacks.Interface;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Minable;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events.Combat;
using Helpers.Events.Status;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class HatchetToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Hatchet Settings")]
        [Tooltip("Tags this hatchet is allowed to affect (e.g., BioObstacle, Vegetation).")]
        public string[] allowedTags;
        // No cost if swing didn't make contact
        [FormerlySerializedAs("swingCooldown")]
        [FormerlySerializedAs("staminaCostPerConnectingSwing")]
        [Tooltip("Number of seconds between swings.")]
        public float normalSwingCooldown = 0.6f;
        public float heavySwingCooldown = 0.4f;

        // public float fromHeldUpChargedSwingCooldown = 0.3f;


        [Tooltip("Tool power sent to HatchetBreakable (compares to its hardness).")]
        public int hatchetPower = 1;

        [SerializeField] Sprite defaultReticleForTool;


        [FormerlySerializedAs("LastSwingTime")] [SerializeField]
        protected float lastSwingTime = -999f;
        public float heavySwingDownFactor = 1.2f;

        float StaminaCostPerNormalConnectingSwing => 20f;

        float StaminaCostPerHeavyConnectingSwing => 20f;


        public override void Use()
        {
            if (attributesManager == null) attributesManager = AttributesManager.Instance;

            if (PlayerMutableStatsManager.Instance.CurrentStamina <= 19.9f)
                PerformToolAction();
            else if (PlayerMutableStatsManager.Instance.CurrentStamina > 19.9f) PerformHeavyChargedToolAction();
        }


        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerPrimaryArmsController;
        }


        public override void Unequip()
        {
            // no-op for now
        }

        public override bool CanInteractWithObject(GameObject target)
        {
            if (target == null) return false;

            // Component gate
            if (target.TryGetComponent<IBreakable>(out _)) return true;

            // Tag gate
            if (allowedTags != null && allowedTags.Length > 0)
            {
                var t = target.tag;
                for (var i = 0; i < allowedTags.Length; i++)
                    if (!string.IsNullOrEmpty(allowedTags[i]) && t == allowedTags[i])
                        return true;
            }

            return false;
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            // Check if the object has a tag that should show inability reticle
            if (tagsWhichShouldShowInabilityReticle != null)
                foreach (var tagName in tagsWhichShouldShowInabilityReticle)
                    if (colliderGameObject.CompareTag(tagName))
                        return reticleForHittable;

            // Default to the normal reticle
            return defaultReticleForTool;
        }

        public override MMFeedbacks GetEquipFeedbacks()
        {
            return equipFeedbacks;
        }
        public override MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }

        void AbortChargeAndReset()
        {
            // Stop the current animation state and clear events
            if (AnimController?.animancerComponent != null)
            {
                var layer = AnimController.animancerComponent.Layers[1];

                // Get the current state and clear all events to prevent OnEnd from firing
                if (layer.CurrentState != null) layer.CurrentState.Events(this).Clear();

                // Disable the layer
                layer.Weight = 0f;
            }

            // Clear action state
            AnimController?.ClearActionState();

            // Reset charge state
            // ChargeTimeElapsed = 0f;
            // ToolIsHeldInChargePosition = false;

            // Trigger release event
            ChargeToolEvent.Trigger(ChargeToolEventType.Release);

            // Return to locomotion
            AnimController?.ReturnToLocomotion();
        }


        public override void ApplyHit(HitType hitType = HitType.Normal)
        {
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera) return;

            if (!Physics.Raycast(
                    mainCamera.transform.position, mainCamera.transform.forward,
                    out var hit, reach, hitMask, QueryTriggerInteraction.Ignore))
                return;


            var applyTimeHit = hit.collider.gameObject;

            var aimTimeHit = SavedAimHitInfo != null && SavedAimHitInfo.Value.collider != null
                ? SavedAimHitInfo.Value.collider.gameObject
                : null;

            // First priority: dedicated component
            if (applyTimeHit.TryGetComponent<IBreakable>(out var breakable))
            {
                // hardness/HP handled inside component
                breakable.ApplyHit(hatchetPower, hit.point, hit.normal, hitType);

                if (applyTimeHit.CompareTag("MiscRigidOrganism")) hitRigidOrganismFeedbacks?.PlayFeedbacks();
            }
            else if (applyTimeHit.TryGetComponent<MyOreNode>(out var oreNode))
            {
                // No apply here – ore nodes are for pickaxe only

                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }
            else if (applyTimeHit.TryGetComponent<IFleshyObject>(out var fleshyObject) ||
                     (aimTimeHit != null && aimTimeHit.TryGetComponent(out fleshyObject)))
            {
                hitFleshyFeedbacks?.PlayFeedbacks();
                fleshyObject.MakeJiggle();
                var contaminationAmt = fleshyObject.BaseBlowbackContaminationAmt;
                if (contaminationAmt > 0f)
                    PlayerStatsEvent.Trigger(
                        PlayerStatsEvent.PlayerStat.CurrentContamination,
                        PlayerStatsEvent.PlayerStatChangeType.Increase,
                        contaminationAmt);
            }
            else if (applyTimeHit.CompareTag("DiggerChunk") || applyTimeHit.CompareTag("MainSceneTerrain"))
            {
                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }
            else if (applyTimeHit.CompareTag("MiscRigidOrganism") ||
                     (aimTimeHit != null && aimTimeHit.CompareTag("MiscRigidOrganism")))
            {
                hitRigidOrganismFeedbacks?.PlayFeedbacks();
            }
            else if (applyTimeHit.CompareTag("EnemyNPC") || (aimTimeHit != null && aimTimeHit.CompareTag("EnemyNPC")))
            {
                var enemyController = applyTimeHit.GetComponentInParent<CreatureController>();

                if (enemyController == null)
                {
                    Debug.LogWarning("HatchetToolPrefab: Hit enemy NPC but no EnemyController found in parents.");
                    return;
                }

                var playerAttack = DetermineCorrectPlayerToolAttack(hitType);


                // Spawn VFX with proper cleanup
                var effectsAndFeedbacks = enemyController.GetEffectsAndFeedbacks();
                GameObject vfx = null;
                if (effectsAndFeedbacks != null) vfx = enemyController.GetEffectsAndFeedbacks().basicHitVFX;

                if (vfx != null)
                {
                    var vfxInstance = Instantiate(vfx, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(vfxInstance, 2f); // Clean up after 2 seconds
                }


                enemyController.ProcessAttackDamage(playerAttack, hit.point);
                if (hitType == HitType.Heavy)

                    Debug.Log("Stamina decreased by: " + StaminaCostPerHeavyConnectingSwing);
                else

                    Debug.Log("Stamina decreased by: " + StaminaCostPerNormalConnectingSwing);
            }
            else
            {
                // No apply here – ore nodes are for pickaxe only

                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }
        }


        public override void PerformToolAction()
        {
            CaptureAim();
            var adjustedCooldown =
                normalSwingCooldown - agilityCooldownSecondsReducePerPoint * (attributesManager.Agility - 1);

            if (Time.time < lastSwingTime + adjustedCooldown) return;
            // swingCooldown -= agilityCooldownSecondsReducePerPoint * (attributesManager.Agility - 1);
            // if (Time.time < lastSwingTime + swingCooldown) return;
            lastSwingTime = Time.time;


            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                StaminaCostPerNormalConnectingSwing);

            StartCoroutine(ApplyAttackLunge(toolAttackProfile.basicAttack, defaultHitDelay));

            if (useMultipleSwings && AnimController.currentToolAnimationSet != null)
            {
                PlaySwingSequence();
            }
            else
            {
                AnimController.PlayToolUseOneShot(speedMultiplier: overallToolSwingSpeedMultiplier);
                StartCoroutine(ApplyNormalHitAfterDelay(defaultHitDelay / overallToolSwingSpeedMultiplier));
            }
        }


        public override void PerformHeavyChargedToolAction()
        {
            CaptureAim();
            var adjustedCooldown =
                heavySwingCooldown - agilityCooldownSecondsReducePerPoint * (attributesManager.Agility - 1);

            if (Time.time < lastSwingTime + adjustedCooldown) return;
            lastSwingTime = Time.time;


            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                StaminaCostPerHeavyConnectingSwing);


            StartCoroutine(ApplyAttackLunge(toolAttackProfile.heavyAttack, swingHeavyHitDelay));

            PlayHeavySwingSequence();
        }


        // Kept to mirror Pickaxe signature – not used by hatchet
        public int GetCurrentTextureIndex()
        {
            return -1;
        }

        public bool CanInteractWithTextureIndex(int terrainIndex)
        {
            return false;
        }
    }
}
