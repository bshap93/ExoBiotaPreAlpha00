using System;
using DG.Tweening;
using FirstPersonPlayer;
using Helpers.Events;
using Helpers.Events.Terminals;
using Helpers.Events.Triggering;
using Manager.SceneManagers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Interface;

namespace LevelConstruct.Elevator
{
    public class ElevatorRootSystem : MonoBehaviour, IRequiresUniqueID,
        MMEventListener<ElevatorRootSystemEvent>,
        MMEventListener<LoadedManagerEvent>
    {
        public string uniqueID;


        [Header("Elevator References")]
        [Tooltip("The cabin Transform that physically moves between floors.")]
        [SerializeField]
        Transform elevatorCabin;

        [Header("Destinations")] public ElevatorDestination[] elevatorDestinations;

        [Header("Movement — Speed & Easing")] [Tooltip("Travel speed in world-units per second.")] [SerializeField]
        float moveSpeed = 4f;

        [Tooltip("Ease applied to the first half of the journey (acceleration).")] [SerializeField]
        Ease easeIn = Ease.InSine;

        [Tooltip("Ease applied to the second half of the journey (deceleration).")] [SerializeField]
        Ease easeOut = Ease.OutSine;

        [Header("Feedbacks")] [Tooltip("Played the moment the elevator begins moving.")] [SerializeField]
        MMFeedbacks departureFeedbacks;

        [Tooltip("Played when the elevator reaches its destination.")] [SerializeField]
        MMFeedbacks arrivalFeedbacks;

        [SerializeField] MMFeedbacks travelFeedbacks;

        [SerializeField] TeleportPlayer teleportPlayer;
        Sequence _activeTween;

        // ── Runtime state ──────────────────────────────────────────────────────

        string _currentDestinationId;
        bool _isMoving;

        ElevatorDestination _lastDestination;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        void OnEnable()
        {
            this.MMEventStartListening<ElevatorRootSystemEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<ElevatorRootSystemEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
            _activeTween?.Kill();
        }

        // ── IRequiresUniqueID ──────────────────────────────────────────────────

        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

        // ── Event listeners ────────────────────────────────────────────────────

        public void OnMMEvent(ElevatorRootSystemEvent eventType)
        {
            if (eventType.ElevatorSystemId != uniqueID) return;
            MoveToDestination(eventType.DestinationId);
        }

        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
                Initialize();
        }
        // When a scene unloads mid-travel, skip straight to the destination
        void OnAdditiveSceneChange(Scene _)
        {
            if (!_isMoving) return;
            SkipToDestination();
        }

        void SkipToDestination()
        {
            // if (teleportPlayer != null)
            //     teleportPlayer.Teleport(player);
        }

        // ── Initialization ─────────────────────────────────────────────────────

        void Initialize()
        {
            if (elevatorCabin == null)
            {
                Debug.LogWarning($"[ElevatorRootSystem] '{name}' has no elevator cabin Transform assigned.");
                return;
            }

            // Try to restore the persisted destination first
            var savedId = ElevatorManager.Instance?.GetDestination(uniqueID);
            if (!string.IsNullOrEmpty(savedId))
            {
                var saved = GetDestinationById(savedId);
                if (saved != null)
                {
                    SnapToDestination(saved, savedId);
                    _lastDestination = saved;
                    return;
                }
            }

            // Fall back to first destination in the array
            if (elevatorDestinations != null && elevatorDestinations.Length > 0)
            {
                var first = elevatorDestinations[0];
                if (first.destinationTransform != null)
                {
                    SnapToDestination(first, first.destinationID);
                    _lastDestination = first;
                }
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        ///     Moves the elevator cabin to <paramref name="destinationId" />.
        ///     Ignored while already moving or if the cabin is already there.
        /// </summary>
        public void MoveToDestination(string destinationId)
        {
            if (_isMoving)
            {
                Debug.Log($"[ElevatorRootSystem] '{name}' is already moving — ignoring new request.");
                return;
            }

            if (destinationId == _currentDestinationId)
            {
                Debug.Log($"[ElevatorRootSystem] '{name}' is already at destination '{destinationId}'.");
                return;
            }

            var dest = GetDestinationById(destinationId);
            if (dest == null)
            {
                Debug.LogWarning($"[ElevatorRootSystem] Destination '{destinationId}' not found on '{name}'.");
                return;
            }

            if (elevatorCabin == null)
            {
                Debug.LogWarning($"[ElevatorRootSystem] No elevator cabin assigned on '{name}'.");
                return;
            }

            _lastDestination = GetDestinationById(_currentDestinationId);

            ExecuteMove(dest, destinationId);
        }

        // Transform player;


        // ── Private helpers ────────────────────────────────────────────────────

        void ExecuteMove(ElevatorDestination dest, string destinationId)
        {
            _activeTween?.Kill();

            var startPos = elevatorCabin.position;
            var endPos = dest.destinationTransform.position;
            var distance = Vector3.Distance(startPos, endPos);
            var duration = distance / Mathf.Max(moveSpeed, 0.01f);
            var halfDur = duration * 0.5f;
            var midPos = Vector3.Lerp(startPos, endPos, 0.5f);

            _isMoving = true;
            departureFeedbacks?.PlayFeedbacks();
            travelFeedbacks?.PlayFeedbacks();


            _activeTween = DOTween.Sequence()
                // Acceleration phase
                .Append(elevatorCabin.DOMove(midPos, halfDur).SetEase(easeIn))
                // Deceleration phase
                .Append(elevatorCabin.DOMove(endPos, halfDur).SetEase(easeOut))
                .OnComplete(() =>
                {
                    _isMoving = false;
                    _currentDestinationId = destinationId;

                    // Persist the new position
                    ElevatorManager.Instance?.SetDestination(uniqueID, destinationId);
                    TriggerSceneUnload(_lastDestination.sceneName);

                    arrivalFeedbacks?.PlayFeedbacks();
                    travelFeedbacks?.StopFeedbacks();
                });
        }

        /// <summary>Instantly places the cabin at a destination, no tween.</summary>
        void SnapToDestination(ElevatorDestination dest, string destinationId)
        {
            elevatorCabin.position = dest.destinationTransform.position;
            _currentDestinationId = destinationId;
        }

        void TriggerSceneUnload(string sceneName)
        {
            MySceneTransitionAdditiveEvent.Trigger(
                MySceneTransitionAdditiveEvent.MySceneTransEventType.Unload, sceneName);
        }

        ElevatorDestination GetDestinationById(string id)
        {
            if (elevatorDestinations == null) return null;
            foreach (var d in elevatorDestinations)
                if (d.destinationID == id)
                    return d;

            return null;
        }

        // ── Nested types ───────────────────────────────────────────────────────

        [Serializable]
        public class ElevatorDestination
        {
            public string destinationID;
            public string destinationName;
            public Transform destinationTransform;
            public string sceneName;
        }
    }
}
