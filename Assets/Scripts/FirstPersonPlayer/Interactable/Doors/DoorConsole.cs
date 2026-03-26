using Animancer;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Machine;
using Helpers.Events.Machinery;
using Inventory;
using LevelConstruct.Interactable.Door;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class DoorConsole : MonoBehaviour, IInteractable
    {
        [FormerlySerializedAs("lockedDoor")] [SerializeField]
        PersistentLockedDoor persistentLockedDoor;
        [SerializeField] MMFeedbacks denyEntryFeedbacks;

        public Vector3 switchOnPosition;
        public Vector3 switchOffPosition;

        public Vector3 switchOnRotation;
        public Vector3 switchOffRotation;

        public UnityEvent onInteract;

        public GameObject switchObject;

        [SerializeField] float interactionDistance = 2f;

        [SerializeField] MMFeedbacks switchFeedbacks;

        void Start()
        {
            // Initialize switch animation based on door state
            AnimateSwitch(!persistentLockedDoor.IsLocked);
        }


        public void Interact()
        {
            if (!CanInteract()) return;


            if (persistentLockedDoor.IsLocked) DoorEvent.Trigger(persistentLockedDoor.uniqueID, DoorEventType.Unlock);
            persistentLockedDoor.OpenDoor();

            BarrierInitializationStateEvent.Trigger(
                BarrierStateEventType.SetNewBarrierState, persistentLockedDoor.uniqueID,
                ConditionalBarrierManager.BarrierInitializationState.ShouldBeInitializedAndTriggered);

            onInteract.Invoke();

            // Update console switch animation
            AnimateSwitch(!persistentLockedDoor.isOpen);
        }
        public void Interact(string param)
        {
            Interact();
        }

        public bool CanInteract()
        {
            if (!persistentLockedDoor.IsLocked)
                return true;

            if (GlobalInventoryManager.Instance.HasKeyForDoor(persistentLockedDoor.keyID))
            {
                persistentLockedDoor.IsLocked = false;
                switchFeedbacks?.PlayFeedbacks();
                return true;
            }

            AlertEvent.Trigger(AlertReason.DoorLocked, "The door is locked. You need a key to open it.");
            denyEntryFeedbacks?.PlayFeedbacks();

            return false;
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }

        public bool IsInteractable()
        {
            return true;
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

        void AnimateSwitch(bool isOn)
        {
            if (switchObject == null) return;

            if (isOn)
            {
                switchObject.transform.localPosition = switchOnPosition;
                switchObject.transform.localEulerAngles = switchOnRotation;
            }
            else
            {
                switchObject.transform.localPosition = switchOffPosition;
                switchObject.transform.localEulerAngles = switchOffRotation;
            }
        }
    }
}
