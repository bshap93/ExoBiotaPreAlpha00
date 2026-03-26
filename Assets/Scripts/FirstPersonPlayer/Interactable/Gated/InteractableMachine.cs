using System;
using System.Collections;
using System.Collections.Generic;
using Dirigible.Input;
using Events;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.ScriptableObjects;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using Inventory;
using Manager;
using Manager.SceneManagers;
using Manager.UI;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.Gated
{
    public class InteractableMachine : MonoBehaviour, IInteractable, IBillboardable, IHoverable, IRequiresUniqueID,
        IGatedInteractable, MMEventListener<GatedMachineInteractionEvent>
    {
        [Serializable]
        public enum MachineState
        {
            None,
            Operating,
            Off,
            Broken
        }

        public Sprite operatingSprite;
        public Sprite offSprite;
        public Sprite brokenSprite;

        [SerializeField] protected float interactionDistance = 2f;

        [SerializeField] protected SceneObjectData sceneObjectData;
        [SerializeField] protected bool isCurrentlyInteractable;
        [SerializeField] protected ObjectiveObject completesObjective;
        [SerializeField] protected GatedMachineInteractionDetails gatedInteractionDetails;
        [Header("Feedbacks")] [SerializeField] protected MMFeedbacks loopedInteractionFeedbacks;
        [SerializeField] protected MMFeedbacks startInteractionFeedbacks;
        [FormerlySerializedAs("gatedMachineType")] [SerializeField]
        protected MachineType machineType;
        [FormerlySerializedAs("state")] [SerializeField]
        protected MachineState currentMachineState = MachineState.Off;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;


        public string uniqueID;
        List<string> _fuelBatteryItemsFound;
        List<string> _toolsFound;
        protected SceneObjectData Data;


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public string GetName()
        {
            return sceneObjectData.Name;
        }
        public Sprite GetIcon()
        {
            return sceneObjectData.Icon;
        }
        public string ShortBlurb()
        {
            return sceneObjectData.ShortBlurb;
        }
        public Sprite GetActionIcon()
        {
            return sceneObjectData.ActionIcon;
        }
        public string GetActionText()
        {
            return sceneObjectData.ActionText;
        }


        public MyBaseItem GetItemByID(string itemID, MoreMountains.InventoryEngine.Inventory inventory)
        {
            foreach (var item in inventory.Content)
            {
                if (item == null) continue;
                if (item.ItemID == itemID)
                {
                    // Option 1: Cast to MyBaseItem if your inventory actually stores those
                    if (item is MyBaseItem myBaseItem)
                        return myBaseItem;

                    // Option 2 (recommended): Reload the definition from Resources
                    var def = Resources.Load<MyBaseItem>($"Items/{itemID}");
                    if (def != null)
                        return def;

                    Debug.LogWarning($"Item '{itemID}' found in inventory but not in Resources/Items/");
                    return null;
                }
            }

            return null;
        }


        public virtual List<string> HasToolForInteractionInInventory()
        {
            var possibleTools = gatedInteractionDetails.requiredToolIDs;
            var foundTools = new List<string>();

            var playerInventory =
                GlobalInventoryManager.Instance.playerInventory;

            var equipmentInventory =
                GlobalInventoryManager.Instance.equipmentInventory;

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, playerInventory);
                if (itemInInventory != null) foundTools.Add(toolID);
            }

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, equipmentInventory);
                if (itemInInventory != null && !foundTools.Contains(toolID))
                    foundTools.Add(toolID);
            }

            return foundTools;
        }
        public virtual bool CanInteract(out GatedInteractionManager.ReasonWhyCannotInteract reason)
        {
            var currentStamina = PlayerMutableStatsManager.Instance.CurrentStamina;

            if (currentStamina - gatedInteractionDetails.staminaCost < 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina;
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina,
                    "You do not have enough stamina to perform this action.",
                    "Not Enough Stamina");

                return false;
            }

            if (!gatedInteractionDetails.requireTools && !gatedInteractionDetails.takesFuelBatteryItem)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
                return true;
            }

            if (gatedInteractionDetails.requireTools && gatedInteractionDetails.takesFuelBatteryItem)
            {
                _toolsFound = HasToolForInteractionInInventory();
                _fuelBatteryItemsFound = HasFuelBatteryForInteractionInInventory();

                if (_toolsFound.Count == 0)
                {
                    reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool;
                    return false;
                }

                if (_fuelBatteryItemsFound.Count == 0)
                {
                    reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryFuelBattery;
                    return false;
                }
            }
            else if (gatedInteractionDetails.requireTools)
            {
                _toolsFound = HasToolForInteractionInInventory();
                if (_toolsFound.Count == 0)
                {
                    reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool;
                    return false;
                }
            }
            else if (gatedInteractionDetails.takesFuelBatteryItem)
            {
                _fuelBatteryItemsFound = HasFuelBatteryForInteractionInInventory();
                if (_fuelBatteryItemsFound.Count == 0)
                {
                    reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryFuelBattery;
                    return false;
                }
            }

            reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
            return true;
        }

        public virtual bool OnHoverStart(GameObject go)
        {
            Sprite spriteToUse;
            switch (currentMachineState)
            {
                case MachineState.Operating:
                    spriteToUse = operatingSprite;
                    break;
                case MachineState.Off:
                    spriteToUse = offSprite;
                    break;
                case MachineState.Broken:
                    spriteToUse = brokenSprite;
                    break;
                default:
                    spriteToUse = sceneObjectData.Icon;
                    break;
            }

            Data = new SceneObjectData(
                machineType.machineName,
                machineType.machineIcon,
                machineType.shortDescription,
                spriteToUse,
                $"{currentMachineState}"
            );

            Data.Id = machineType.machineID;

            BillboardEvent.Trigger(Data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId, additionalInfoText: "to " + GetActionText()
                );

            return true;
        }
        public virtual bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public virtual bool OnHoverEnd(GameObject go)
        {
            if (Data == null) Data = SceneObjectData.Empty();
            BillboardEvent.Trigger(Data, BillboardEventType.Hide);
            BillboardEvent.Trigger(Data, BillboardEventType.Hide);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId);

            return true;
        }


        public virtual void Interact()
        {
            if (!CanInteract(out var reason))
            {
                if (reason == GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool)
                    AlertEvent.Trigger(
                        AlertReason.LackToolForInteraction, "You need the appropriate axe to destroy this organism.",
                        "Lacking Necessary Tool");
                else if (reason == GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina)
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughStamina, "You do not have enough stamina to perform this action.",
                        "Not Enough Stamina");

                return;
            }


            GatedMachineInteractionEvent.Trigger(
                GatedInteractionEventType.TriggerGateUI, gatedInteractionDetails, uniqueID, _fuelBatteryItemsFound,
                _toolsFound);
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }
        public virtual void OnInteractionStart()
        {
            startInteractionFeedbacks?.PlayFeedbacks();
            loopedInteractionFeedbacks?.PlayFeedbacks();
        }
        public virtual void OnInteractionEnd(string subjectUniquedID)
        {
            loopedInteractionFeedbacks?.StopFeedbacks();
            if (subjectUniquedID != uniqueID) return;

            AlertEvent.Trigger(
                AlertReason.MachineInteraction,
                "Machine interaction complete.",
                "Interaction Complete");


            if (completesObjective != null)
                ObjectiveEvent.Trigger(completesObjective.objectiveId, ObjectiveEventType.ObjectiveCompleted);
        }
        public bool CanInteract()
        {
            return CanInteract(out _);
        }
        public bool IsInteractable()
        {
            return isCurrentlyInteractable;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        public string UniqueID
        {
            get => uniqueID;
            set => uniqueID = value;
        }
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
        public void OnMMEvent(GatedMachineInteractionEvent eventType)
        {
            if (eventType.SubjectUniqueID != uniqueID)
                return; // Ignore events for other interactables

            if (eventType.EventType == GatedInteractionEventType.StartInteraction)
                OnInteractionStart();
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
                OnInteractionEnd(eventType.SubjectUniqueID);
        }

        protected virtual IEnumerator InitializeAfterMachineStateManager()
        {
            yield return null;

            if (MachineStateManager.Instance != null)
            {
                var machineState =
                    MachineStateManager.Instance.GetMachineStateByID(uniqueID);

                if (machineState != MachineState.None)
                    currentMachineState = machineState;
            }
            else
            {
                Debug.LogWarning(
                    $"MachineStateManager instance not found when initializing {machineType.machineName}. Using default state.");
            }
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif

        public List<string> HasFuelBatteryForInteractionInInventory()
        {
            var possibleFuelBatteries = gatedInteractionDetails.compatibleFuelBatteryIDs;
            var foundFuelBatteries = new List<string>();

            var playerInventory =
                GlobalInventoryManager.Instance.playerInventory;

            var equipmentInventory =
                GlobalInventoryManager.Instance.equipmentInventory;

            foreach (var fuelBatteryID in possibleFuelBatteries)
            {
                var itemInInventory = GetItemByID(fuelBatteryID, playerInventory);
                if (itemInInventory != null) foundFuelBatteries.Add(fuelBatteryID);
            }

            foreach (var fuelBatteryID in possibleFuelBatteries)
            {
                var itemInInventory = GetItemByID(fuelBatteryID, equipmentInventory);
                if (itemInInventory != null && !foundFuelBatteries.Contains(fuelBatteryID))
                    foundFuelBatteries.Add(fuelBatteryID);
            }

            return foundFuelBatteries;
        }
    }
}
