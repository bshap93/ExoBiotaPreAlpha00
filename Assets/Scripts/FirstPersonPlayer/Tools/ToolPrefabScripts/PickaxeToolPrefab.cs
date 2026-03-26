using System;
using System.Linq;
using Domains.Gameplay.Mining.Scripts;
using Feedbacks.Interface;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using Helpers.Events.Status;
using LevelConstruct.Highlighting;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class PickaxeToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Mining Settings")] public float miningCooldown = 1f;

        public int hardnessCanBreak = 1;
        public float highlighingRange;

        // No cost if swing didn't make contact
        // [FormerlySerializedAs("staminaCostPerConnectingSwing")] [SerializeField]
        // float baseStaminaCostPerConnectingSwing = 1f;
        // public float baseStaminaCostPerHeavyConnectingSwing = 2f;

        [SerializeField] Sprite defaultReticleForTool;


        // Not in abstract base class
        [Header("Allowed Textures")] public int[] allowedTerrainTextureIndices;

        [SerializeField] MMFeedbacks equippedFeedbacks;


        [SerializeField] GameObject unequippedEffectPrefab;
        [SerializeField] bool showHighlights = true;

        [Header("Layer Settings")] [SerializeField]
        LayerMask minableLayers = -1; // Default: all layers


        [Tooltip("Tool power sent to breakables (compares to their hardness).")]
        public int pickaxePower = 1;
        float _checkObjectsCooldown;

        bool _hasValidHit;

        float _lastSwingTime = -999f;

        RaycastHit _pendingHit;
        float StaminaPerConnectingSwing
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                var baseEnergyCost = toolAttackProfile.basicAttack.baseStaminaCost;
                if (attrMgr == null) return baseEnergyCost;

                var agility = attrMgr.Agility;
                var reduction = toolAttackProfile.agilityReductionFactor * agility; // Example: 0.05
                var finalCost = baseEnergyCost * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }

        float StaminaCostPerNormalConnectingSwing
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                var baseEnergyCost = toolAttackProfile.basicAttack.baseStaminaCost;
                if (attrMgr == null) return baseEnergyCost;

                var agility = attrMgr.Agility;
                var reduction = toolAttackProfile.agilityReductionFactor * (agility - 1); // Example: 0.05
                var finalCost = baseEnergyCost * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }

        float StaminaCostPerHeavyConnectingSwing
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                var baseEnergyCost = toolAttackProfile.heavyAttack.baseStaminaCost;
                if (attrMgr == null) return baseEnergyCost;
                var agility = attrMgr.Agility;
                var reduction = toolAttackProfile.agilityReductionFactor * (agility - 1); // Example: 0.05
                var finalCost = baseEnergyCost * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }


        void Update()
        {
            if (showHighlights)
            {
                _checkObjectsCooldown -= Time.deltaTime;
                if (_checkObjectsCooldown <= 0f)
                {
                    foreach (var minable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                                 .OfType<IMinable>())
                    {
                        var controller = (minable as Component)?.GetComponent<HighlightEffectController>();
                        if (controller == null) continue;

                        //Check if the object is on an allowed layer
                        if (!IsOnAllowedLayer(controller.gameObject)) continue;

                        var inRange = IsMinableWithinHighlightingRange(controller.gameObject);
                        controller.SetHighlighted(inRange);
                        controller.SetTargetVisible(inRange);
                    }

                    _checkObjectsCooldown = 0.25f;
                }
            }
        }


        void OnEnable()
        {
            foreach (var minable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IMinable>())
            {
                var controller = (minable as Component)?.GetComponent<HighlightEffectController>();

                if (controller != null && IsMinableWithinHighlightingRange(controller.gameObject))
                    //if (controller != null)
                {
                    controller.SetHighlighted(true);
                    controller.SetTargetVisible(true);
                }
            }
        }


        /* --------- IRuntimeTool --------- */
        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerPrimaryArmsController;
        }

        public override void Use()
        {
            if (PlayerMutableStatsManager.Instance.CurrentStamina < StaminaPerConnectingSwing)
            {
                // Not enough stamina
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina, "Not enough stamina to use pickaxe.", "Insufficient Stamina");

                return;
            }

            if (attributesManager == null) attributesManager = AttributesManager.Instance;

            PerformToolAction();
        }
        public override void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }

        public override void Unequip()
        {
            foreach (var minable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IMinable>())
            {
                var controller = (minable as Component)?.GetComponent<HighlightEffectController>();
                if (controller != null)
                {
                    controller.SetHighlighted(false);
                    controller.SetTargetVisible(false);
                }
            }
        }

        public override bool CanInteractWithObject(GameObject target)
        {
            // ✅ 1. Check if it's an ore/minable object
            var minable = target.GetComponent<IMinable>();
            if (minable != null) return true;

            var breakableBio = target.GetComponent<IBreakable>();
            if (breakableBio != null) return true;

            return false;
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return defaultReticleForTool;
        }

        /* --------- Core Mining --------- */
        public override MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;
        }
        public override MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }


        bool IsOnAllowedLayer(GameObject obj)
        {
            return (minableLayers.value & (1 << obj.layer)) != 0;
        }

        bool IsMinableWithinHighlightingRange(GameObject minableObj)
        {
            if (Camera.main != null)
            {
                var camerTransform = Camera.main.transform;
                var minableTransform = minableObj.transform;

                var sqrDistance = (camerTransform.position - minableTransform.position).sqrMagnitude;

                return sqrDistance <= highlighingRange * highlighingRange;
            }

            return false;
        }

        public bool CanInteractWithTextureIndex(int index)
        {
            if (index < 0) return false;
            if (allowedTerrainTextureIndices == null || allowedTerrainTextureIndices.Length == 0) return true;
            foreach (var allowed in allowedTerrainTextureIndices)
                if (index == allowed)
                    return true;

            return false;
        }

        public override void ApplyHit(HitType hitType = HitType.Normal)
        {
            // Use the stored hit from when button was pressed


            var hit = _pendingHit; // ← Use stored data
            if (!hit.collider) return;
            var go = hit.collider.gameObject;


            if (!_hasValidHit)
            {
                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
                return;
            }

            // First priority: dedicated component
            if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                // hardness/HP handled inside component
                breakable.ApplyHit(pickaxePower, hit.point, hit.normal);


                // SpawnFxForConnectingHit(hit.point, hit.normal);
                return;
            }

            var minable = go.GetComponent<IMinable>();
            if (minable != null)
                if (minable.GetHardness() <= hardnessCanBreak)
                    minable.MinableMineHit();


            if (go.TryGetComponent<IFleshyObject>(out var fleshyObject))
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
            else if (go.TryGetComponent<BreakableStoneBarrier>(out var breakableStone))
            {
                breakableStone.ApplyHit(pickaxePower, hit.point, hit.normal, hitType);
                hitRockFeedbacks?.PlayFeedbacks();
            }
            else if (go.CompareTag("MiscRigidOrganism"))
            {
                hitRigidOrganismFeedbacks?.PlayFeedbacks();
            }

            if (go.CompareTag("EnemyNPC"))
            {
                var enemyController = go.GetComponentInParent<CreatureController>();

                if (enemyController == null)
                {
                    Debug.LogWarning("HatchetToolPrefab: Hit enemy NPC but no EnemyController found in parents.");
                    return;
                }

                var playerAttack = DetermineCorrectPlayerToolAttack(hitType);


                // Spawn VFX with proper cleanup
                var vfx = enemyController.GetEffectsAndFeedbacks().basicHitVFX;
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
        }


        public override void PerformToolAction()
        {
            var effectiveCooldown = miningCooldown - agilityCooldownSecondsReducePerPoint * attributesManager.Agility;

            // miningCooldown -= agilityCooldownSecondsReducePerPoint * attributesManager.Agility;
            // Check cooldown first
            if (Time.time < _lastSwingTime + effectiveCooldown) return;

            _lastSwingTime = Time.time; // ← Move this up here

            // IMMEDIATELY raycast to capture target
            _hasValidHit = Physics.Raycast(
                mainCamera.transform.position,
                mainCamera.transform.forward,
                out _pendingHit,
                reach,
                hitMask,
                QueryTriggerInteraction.Ignore
            );


            _lastSwingTime = Time.time;
            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                StaminaPerConnectingSwing);

            PlaySwingSequence();
        }
        // public override void PerformPartiallyChargedToolAction()
        // {
        //     throw new NotImplementedException();
        // }
        public override void PerformHeavyChargedToolAction()
        {
            throw new NotImplementedException();
        }


        /* --------- Helpers --------- */
    }
}
