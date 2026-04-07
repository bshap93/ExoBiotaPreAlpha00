using System;
using System.Collections;
using Animancer;
using DG.Tweening;
using Helpers.Events;
using Helpers.Events.Machine;
using LevelConstruct.Highlighting;
using Manager.SceneManagers;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace LevelConstruct.Interactable.Door
{
    public class PersistentLockedDoor : MonoBehaviour, IRequiresUniqueID, MMEventListener<DoorEvent>,
        MMEventListener<LoadedManagerEvent>
    {
        public string uniqueID;


        public string keyID;

        [FormerlySerializedAs("navMeshLink")] [SerializeField]
        protected NavMeshLink[] navMeshLinks;


        [SerializeField] bool usesAnimationClips = true;
        [ShowIf("usesAnimationClips")] [SerializeField]
        AnimancerComponent animancerComponent;


        [Header("Override Lock State")] [SerializeField]
        bool overrideLockState;
        [SerializeField] bool defaultLockState;
        [ShowIf("overrideLockState")] [SerializeField]
        bool startLocked = true;

        public ConditionalBarrierManager.BarrierInitializationState initialBarrierStateInitializationState;


        [Header("Animation Clips")] [ShowIf("usesAnimationClips")] [SerializeField]
        AnimationClip openAnimation;
        [ShowIf("usesAnimationClips")] [SerializeField]
        AnimationClip closeAnimation;
        [ShowIf("usesAnimationClips")] [SerializeField]
        AnimationClip openedAnimation;

        [Header("DOTween Swing Settings")] [SerializeField]
        bool usesDotWeenForSwing;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        bool doubleDoors = true;

        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        GameObject leftDoor;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        GameObject rightDoor;

        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 leftDoorOpenRotation;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 rightDoorOpenRotation;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 leftDoorCloseRotation;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 rightDoorCloseRotation;

        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 leftDoorOpenPosition;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 rightDoorOpenPosition;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 leftDoorClosePosition;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Vector3 rightDoorClosePosition;

        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        float swingDuration = 1f;
        [ShowIf("usesDotWeenForSwing")] [SerializeField]
        Ease swingEase = Ease.InOutSine;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks openFeedbacks;
        [SerializeField] MMFeedbacks closeFeedbacks;
        [Header("Highlighting")] [SerializeField]
        HighlightEffectController associatedHighlightEffectController;


        public bool isOpen;
        DoorManager _doorManager;
        public bool IsLocked { get; set; }

        void Start()
        {
            if (overrideLockState) IsLocked = startLocked;

            if (IsLocked) associatedHighlightEffectController.SetSecondaryStateHighlightColor();


            foreach (var navMeshLink in navMeshLinks)
                if (navMeshLink != null)
                    navMeshLink.enabled = isOpen;

            if (DoorManager.Instance != null)
                if (DoorManager.Instance.GetDoorOpenState(uniqueID) == DoorManager.DoorOpenState.Open)
                    OpenDoor();

            StartCoroutine(InitializeAfterBarrierStateManager());
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

        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
        public void OnMMEvent(DoorEvent eventType)
        {
            if (!(eventType.UniqueId == uniqueID)) return;

            switch (eventType.EventType)
            {
                case DoorEventType.Unlock:
                    IsLocked = false;
                    associatedHighlightEffectController.SetPrimaryStateHighlightColor();
                    break;
                case DoorEventType.Lock:
                    IsLocked = true;
                    associatedHighlightEffectController.SetSecondaryStateHighlightColor();
                    break;
                case DoorEventType.Open:
                    OpenDoor();
                    break;
                case DoorEventType.Close:
                    CloseDoor();
                    break;
                default:
                    Debug.LogWarning("Unhandled DoorEventType: " + eventType.EventType);
                    break;
            }
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType != ManagerType.All)
                return;

            _doorManager = DoorManager.Instance;

            if (_doorManager.DoorHasLockedState(uniqueID))
            {
                var lockState = _doorManager.GetDoorLockState(uniqueID);
                IsLocked = lockState == DoorManager.DoorLockState.Locked;
                if (IsLocked) associatedHighlightEffectController.SetSecondaryStateHighlightColor();
                else associatedHighlightEffectController.SetPrimaryStateHighlightColor();
            }
            else
            {
                IsLocked = defaultLockState;
                if (IsLocked) associatedHighlightEffectController.SetSecondaryStateHighlightColor();
                else associatedHighlightEffectController.SetPrimaryStateHighlightColor();
            }
        }

        IEnumerator InitializeAfterBarrierStateManager()
        {
            yield return null;

            var barrierStateManager = ConditionalBarrierManager.Instance;
            if (barrierStateManager != null)
            {
                var barrierState = barrierStateManager.GetBarrierInitializationState(uniqueID);
                if (barrierState == ConditionalBarrierManager.BarrierInitializationState.None)
                    barrierState = initialBarrierStateInitializationState;


                if (barrierState == ConditionalBarrierManager.BarrierInitializationState.ShouldBeDestroyed)
                {
                    Destroy(gameObject);
                }
                else if (barrierState == ConditionalBarrierManager.BarrierInitializationState
                             .ShouldBeInitializedAndTriggered)
                {
                    OpenDoor();
                    IsLocked = false;
                    associatedHighlightEffectController.SetSecondaryStateHighlightColor();
                }
            }
        }

        public void ToggleDoor()
        {
            if (isOpen)
                CloseDoor();
            else
                OpenDoor();
        }


        public void OpenDoor()
        {
            if (isOpen) return;

            if (IsLocked) return;

            if (usesAnimationClips && openAnimation != null)
            {
                var openState = animancerComponent.Play(openAnimation);

                openFeedbacks?.PlayFeedbacks();

                openState.Events(this).OnEnd = () =>
                {
                    // When fully open, idle in opened pose (optional)
                    if (openedAnimation != null)
                        animancerComponent.Play(openedAnimation);

                    isOpen = true;

                    foreach (var navMeshLink in navMeshLinks)
                        if (navMeshLink != null)
                            navMeshLink.enabled = isOpen;
                };
            }
            else if (usesDotWeenForSwing)
            {
                openFeedbacks?.PlayFeedbacks();

                rightDoor.transform.DOLocalRotate(rightDoorOpenRotation, swingDuration).SetEase(swingEase);
                rightDoor.transform.DOLocalMove(rightDoorOpenPosition, swingDuration).SetEase(swingEase);

                if (doubleDoors)
                {
                    leftDoor.transform.DOLocalRotate(leftDoorOpenRotation, swingDuration).SetEase(swingEase);
                    leftDoor.transform.DOLocalMove(leftDoorOpenPosition, swingDuration).SetEase(swingEase);
                }

                isOpen = true;

                foreach (var navMeshLink in navMeshLinks)
                    if (navMeshLink != null)
                        navMeshLink.enabled = isOpen;
            }
        }

        public void CloseDoor()
        {
            if (!isOpen) return;

            if (usesAnimationClips && closeAnimation != null)
            {
                closeFeedbacks?.PlayFeedbacks();
                var closeState = animancerComponent.Play(closeAnimation);
                closeState.Events(this).OnEnd = () =>
                {
                    isOpen = false;
                    foreach (var navMeshLink in navMeshLinks)
                        if (navMeshLink != null)
                            navMeshLink.enabled = isOpen;

                    closeState.Stop();
                };
            }
            else if (usesDotWeenForSwing)
            {
                closeFeedbacks?.PlayFeedbacks();
                rightDoor.transform.DOLocalRotate(rightDoorCloseRotation, swingDuration).SetEase(swingEase);
                rightDoor.transform.DOLocalMove(rightDoorClosePosition, swingDuration).SetEase(swingEase);

                if (doubleDoors)
                {
                    leftDoor.transform.DOLocalRotate(leftDoorCloseRotation, swingDuration).SetEase(swingEase);
                    leftDoor.transform.DOLocalMove(leftDoorClosePosition, swingDuration).SetEase(swingEase);
                }

                isOpen = false;

                foreach (var navMeshLink in navMeshLinks)
                    if (navMeshLink != null)
                        navMeshLink.enabled = isOpen;
            }

            // isOpen = false;
        }
    }
}
