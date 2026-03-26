using Helpers.Events;
using Helpers.Events.Machine;
using LevelConstruct.Highlighting;
using Manager.SceneManagers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class GenericLockedDoor : GenericDoor, MMEventListener<DoorEvent>, MMEventListener<LoadedManagerEvent>
    {
        [Header("Feedbacks")] [SerializeField] MMFeedbacks lockedDoorWasTriedFeedbacks;
        [SerializeField] MMFeedbacks unlockedDoorFeedbacks;

        [Header("Highlighting")] [SerializeField]
        HighlightEffectController associatedHighlightEffectController;

        [Header("Override Lock State")] [SerializeField]
        bool overrideLockState;
        [ShowIf("overrideLockState")] [SerializeField]
        bool startLocked = true;
        [Header("Door Light")] [SerializeField]
        bool hasDoorLight;
        [ShowIf("hasDoorLight")] [SerializeField]
        Light doorLight;
        [ShowIf("hasDoorLight")] [SerializeField]
        Color lockedLightColor;
        [ShowIf("hasDoorLight")] [SerializeField]
        Color unlockedLightColor;

        DoorManager _doorManager;
        bool _isLocked;

        DoorManager.DoorLockState _lockState;

        void Start()
        {
            if (overrideLockState) _isLocked = startLocked;

            if (_isLocked) associatedHighlightEffectController.SetSecondaryStateHighlightColor();
        }

        void OnEnable()
        {
            this.MMEventStartListening<DoorEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<DoorEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
        }
        public void OnMMEvent(DoorEvent eventType)
        {
            if (eventType.UniqueId != uniqueID) return;
            switch (eventType.EventType)
            {
                case DoorEventType.Unlock:
                    _isLocked = false;
                    associatedHighlightEffectController.SetPrimaryStateHighlightColor();
                    unlockedDoorFeedbacks?.PlayFeedbacks();
                    break;
                case DoorEventType.Lock:
                    _isLocked = true;
                    associatedHighlightEffectController.SetSecondaryStateHighlightColor();
                    break;
                case DoorEventType.Open:
                    OpenDoor();
                    break;
                case DoorEventType.Close:
                    CloseDoor();
                    break;
            }
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType != ManagerType.All)
                return;

            _doorManager = DoorManager.Instance;
            if (_doorManager == null) return;

            if (_doorManager.DoorHasLockedState(uniqueID))
            {
                var lockState = _doorManager.GetDoorLockState(uniqueID);
                _isLocked = lockState == DoorManager.DoorLockState.Locked;
                if (_isLocked) associatedHighlightEffectController.SetSecondaryStateHighlightColor();
                else associatedHighlightEffectController.SetPrimaryStateHighlightColor();
            }
        }

        public override void Interact()
        {
            if (_isLocked)
            {
                lockedDoorWasTriedFeedbacks?.PlayFeedbacks();
                return;
            }

            base.Interact();
        }
    }
}
