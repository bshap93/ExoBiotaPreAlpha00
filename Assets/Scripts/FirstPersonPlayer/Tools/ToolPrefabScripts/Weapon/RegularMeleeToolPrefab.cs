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

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class RegularMeleeToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        public string[] allowedTags;
        [FormerlySerializedAs("normalAttackCooldown")]
        public float attackCooldown = 0.6f;
        // public float heavyAttackAdditionalCooldown = 0.2f;

        public int spearPower = 1;

        [SerializeField] Sprite defaultReticleForTool;

        [SerializeField] protected float lastAttackTime = -999f;

        [SerializeField] float staminaHeavyAttackThreshold = 19.9f;

        float StaminaCostPerNormalAttack => 20f;

        float StaminaCostPerHeavyAttack => 20f;

        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerPrimaryArmsController;
        }
        public override void Use()
        {
            if (attributesManager == null) attributesManager = AttributesManager.Instance;


            if (PlayerMutableStatsManager.Instance.CurrentStamina <= staminaHeavyAttackThreshold)
                PerformToolAction();
            else if (PlayerMutableStatsManager.Instance.CurrentStamina > staminaHeavyAttackThreshold)
                PerformHeavyChargedToolAction();
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


            // do damage to valid targets
            if (applyTimeHit.TryGetComponent<IBreakable>(out var breakable))
            {
                // hardness/HP handled inside component
                breakable.ApplyHit(spearPower, hit.point, hit.normal, hitType);

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
                // if (hitType == HitType.Heavy)
                //
                //     Debug.Log("Stamina decreased by: " + StaminaCostPerHeavyAttack);
                // else
                //
                //     Debug.Log("Stamina decreased by: " + StaminaCostPerNormalAttack);
            }
            else
            {
                // No apply here – ore nodes are for pickaxe only

                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }

            Debug.Log($"[BaseSpearToolPrefab] Hit object: {applyTimeHit.name}, tag: {applyTimeHit.tag}");
        }
        public override void PerformToolAction()
        {
            attackCooldown -= agilityCooldownSecondsReducePerPoint * (attributesManager.Agility - 1);
            if (Time.time < lastAttackTime + attackCooldown) return;
            lastAttackTime = Time.time;

            var uniqueCreatureId = PlayerInteraction.Instance.CreatureControllerCurrentlyInRangeAimed();

            if (uniqueCreatureId != null)
                PlayerStartsAttackEvent.Trigger(toolAttackProfile.basicAttack, uniqueCreatureId);

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
            var adjustedCooldown =
                attackCooldown - agilityCooldownSecondsReducePerPoint * (attributesManager.Agility - 1);

            if (Time.time < lastAttackTime + adjustedCooldown) return;
            lastAttackTime = Time.time;

            var uniqueCreatureId = PlayerInteraction.Instance.CreatureControllerCurrentlyInRangeAimed();

            if (uniqueCreatureId != null)
                PlayerStartsAttackEvent.Trigger(toolAttackProfile.basicAttack, uniqueCreatureId);

            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                StaminaCostPerHeavyAttack);

            StartCoroutine(ApplyAttackLunge(toolAttackProfile.heavyAttack, swingHeavyHitDelay));

            // Heavy attack logic goes here.

            PlayHeavySwingSequence();
        }
    }
}
