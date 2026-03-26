using System;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.Inventory;
using Helpers.Events.Machinery;
using Manager.DialogueScene;
using MoreMountains.Tools;
using UnityEngine;

namespace NewScript
{
    public class SpontaneousEventHandler : MonoBehaviour,
        MMEventListener<SpontaneousTriggerEvent>
    {
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(SpontaneousTriggerEvent e)
        {
            if (string.IsNullOrEmpty(e.StringParameter))
                return;

            // Expected format: "ElevatorEvent:SetNextFloor"
            var parts = e.StringParameter.Split(':');
            if (parts.Length != 2)
            {
                Debug.LogWarning($"[SpontaneousEventHandler] Invalid event string: {e.StringParameter}");
                return;
            }

            var eventFamily = parts[0];
            var eventName = parts[1];

            switch (eventFamily)
            {
                case "ElevatorEvent":
                    HandleElevatorEvent(eventName, e);
                    break;
                case "ItemPickerEvent":
                    HandleItemPickerEvent(eventName, e);
                    break;
                case "ControlsHelpEvent":
                    HandleControlsHelpEvent(eventName, e);
                    break;
                case "DialogueEvent":
                    HandleFirstPersonDialogueEvent(eventName, e);
                    break;
                    


                // future-proofing
                // case "DoorEvent":
                // case "PowerGridEvent":
            }
        }
        
        void HandleFirstPersonDialogueEvent(string eventTypeName, SpontaneousTriggerEvent e)
        {
            if (!Enum.TryParse(eventTypeName, out FirstPersonDialogueEventType dialogueEventType))
            {
                Debug.LogWarning(
                    $"[SpontaneousEventHandler] Unknown FirstPersonDialogueEventType: {eventTypeName}");

                return;
            }
            
            FirstPersonDialogueEvent.Trigger(
                dialogueEventType,
                e.UniqueID,
                e.SecondaryStringParameter
            );
            
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
 

        }

        void HandleControlsHelpEvent(string eventTypeName, SpontaneousTriggerEvent e)
        {
            if (!Enum.TryParse(eventTypeName, out ControlHelpEventType controlsHelpEventType))
            {
                Debug.LogWarning(
                    $"[SpontaneousEventHandler] Unknown ControlsHelpEventType: {eventTypeName}");

                return;
            }


            ControlsHelpEvent.Trigger(
                controlsHelpEventType,
                e.IntParameter
            );
        }

        void HandleElevatorEvent(string eventName, SpontaneousTriggerEvent e)
        {
            if (!Enum.TryParse(eventName, out ElevatorEventType elevatorEventType))
            {
                Debug.LogWarning(
                    $"[SpontaneousEventHandler] Unknown ElevatorEventType: {eventName}");

                return;
            }

            ElevatorEvent.Trigger(
                e.UniqueID,
                elevatorEventType,
                e.IntParameter
            );
        }

        void HandleItemPickerEvent(string eventName, SpontaneousTriggerEvent e)
        {
            if (!Enum.TryParse(eventName, out ItemPickerEvent.ItemPickerEventType itemPickerEventType))
            {
                Debug.LogWarning(
                    $"[SpontaneousEventHandler] Unknown ItemPickerEventType: {eventName}");

                return;
            }

            ItemPickerEvent.Trigger(
                itemPickerEventType,
                e.UniqueID
            );
        }
    }
}
