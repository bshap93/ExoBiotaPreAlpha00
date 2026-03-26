using System.Reflection;
using FirstPersonPlayer.Tools.Animation;
using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using FirstPersonPlayer.Tools.ToolPrefabScripts.Container;
using Helpers.Events;
using LevelConstruct.Interactable.ItemInteractables;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Disposable
{
    [RequireComponent(typeof(ToolBob))]
    [RequireComponent(typeof(ToolObjectController))]
    public class DisposableReagentCanisterPrefab : ApplicatorToolPrefab
    {
        [Header("Disposable Reagent Canister Settings")]
        // [SerializeField]
        // float applyDuration = 1.25f; // seconds to pour reagent
        [SerializeField]
        float reach = 2.5f; // max raycast distance
        [SerializeField] int numberOfUses = 1; // disposable: default 1 use

        [Header("Feedbacks")] [Header("Feedbacks")] [SerializeField]
        MMFeedbacks startApplyFeedbacks;
        [SerializeField] MMFeedbacks completeApplyFeedbacks;

        [SerializeField] MMFeedbacks equippedFeedbacks;
        [SerializeField] MMFeedbacks unequippedFeedbacks;

        [SerializeField] LiquidContainerSObject liquidContainerSObject;
        [SerializeField] LayerMask hitMask = ~0; // filter if desired


        HarvestableItemPickerHelper _currentTarget;

        void OnEnable()
        {
            if (MainCamera == null && Camera.main != null)
                MainCamera = Camera.main;

            ResetProgress();
        }

        public override void Initialize(PlayerEquipment owner)
        {
            // if (liquidContainerSObject == null)
            //     liquidContainerSObject = owner.CurrentToolSo as LiquidContainerSObject;
            //
            // if (liquidContainerSObject != null)
            //     liquidType = liquidContainerSObject.containedLiquidType;
        }


        public override void Use()
        {
            PerformToolAction();
        }

        public override bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return colliderGameObject != null &&
                   colliderGameObject.GetComponentInParent<HarvestableItemPickerHelper>() != null;
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return defaultReticleForTool;
        }

        public override bool CanAbortAction()
        {
            return true;
        }

        public override MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;
        }

        public override MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }
        public override void PerformToolAction()
        {
            if (MainCamera == null) return;

            // Raycast forward
            if (!Physics.Raycast(
                    MainCamera.transform.position, MainCamera.transform.forward,
                    out var hit, reach, hitMask, QueryTriggerInteraction.Ignore))
            {
                AbortAction();
                return;
            }

            var core = hit.collider.GetComponentInParent<HarvestableItemPickerHelper>();
            if (core == null)
            {
                AbortAction();
                return;
            }

            // New target? Reset timer & play start feedback
            if (!ReferenceEquals(core, _currentTarget))
            {
                _currentTarget = core;
                _timer = 0f;
            }


            // Complete pour
            TryApplyReagent(core);
        }

        void FinishApplication()
        {
            completeApplyFeedbacks?.PlayFeedbacks();


            numberOfUses--;
            if (numberOfUses <= 0) ConsumeAndRemove();
        }

        void AbortAction()
        {
            if (CanAbortAction())
            {
                _currentTarget = null;
                _timer = 0f;
                _useHeldThisFrame = false;
            }
        }

        void ResetProgress()
        {
            _timer = 0f;
        }

        void TryApplyReagent(HarvestableItemPickerHelper core)
        {
            if (liquidType is not ReagentType reagentType)
                return;

            switch (core.harvestableState)
            {
                case HarvestableItemPickerHelper.HarvestableState.Undetatched:
                    if (reagentType.reagentClass == ReagentClass.Catalyst)
                    {
                        core.ApplyCatalyst(reagentType);
                        AlertEvent.Trigger(
                            AlertReason.SuccessfulChemicalApplication, "Applied catalyst to Rhizomic Core.",
                            "Applied Chemical");

                        startApplyFeedbacks?.PlayFeedbacks();
                        FinishApplication();
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.InvalidAction,
                            "Solvent has no effect before catalyst.", "Invalid Application");
                    }

                    break;

                case HarvestableItemPickerHelper.HarvestableState.HadCatalystApplied:
                    if (reagentType.reagentClass == ReagentClass.Solvent)
                    {
                        startApplyFeedbacks?.PlayFeedbacks();
                        core.ApplySolvent(reagentType);
                        FinishApplication();
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.InvalidAction,
                            "Catalyst already applied.", "Invalid Application");
                    }

                    break;

                case HarvestableItemPickerHelper.HarvestableState.Dissolved:
                    AlertEvent.Trigger(
                        AlertReason.InvalidAction,
                        "Core is already dissolved.", "No Effect");

                    break;
            }
        }

        void ConsumeAndRemove()
        {
            // Unequip from PlayerEquipment (will destroy prefab instance)
            var equipment = GetComponentInParent<PlayerEquipment>();
            if (equipment != null)
                equipment.GetType().GetMethod(
                        "UnequipTool",
                        BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(equipment, null);

            // Remove item from inventory
            if (liquidContainerSObject != null)
            {
                // var playerInventory = GlobalInventoryManager.Instance.playerInventory;
                var rHandInventory = MoreMountains.InventoryEngine.Inventory.FindInventory(
                    liquidContainerSObject.TargetEquipmentInventoryName, "Player1");


                liquidContainerSObject.UnEquip("Player1");
                // playerInventory?.RemoveItemByID(liquidContainerSObject.ItemID, 1);


                rHandInventory?.RemoveItemByID(liquidContainerSObject.ItemID, 1);
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null, liquidContainerSObject.TargetEquipmentInventoryName,
                    liquidContainerSObject, 1, 0, "Player1");
            }
        }

#pragma warning disable CS0414 // Field is assigned but its value is never used
        float _timer;
        bool _useHeldThisFrame;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    }
}
