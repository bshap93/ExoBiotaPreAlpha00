using Events;
using Manager.SceneManagers.Dock;
using MoreMountains.Tools;
using UnityEngine;

namespace Overview.OverviewMode
{
    public enum OverviewState
    {
        Overview,
        AtTrader,
        AtNpcResidence,
        AtLaboratory,
        AtMiscNpc
    }

    public class OverviewStateMachine : MonoBehaviour, MMEventListener<OverviewLocationEvent>
    {
        public static OverviewStateMachine Instance;
        [SerializeField] private GameObject cameraTarget;
        private bool _hasReturnPose;
        private Vector3 _originalCameraPosition;
        private Vector3 _originalCameraRotation;


        // NEW: last overview pose
        private Vector3 _returnCameraPosition;
        private Quaternion _returnCameraRotation;


        public OverviewState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // Initialize the state machine with a default state
            CurrentState = OverviewState.Overview;
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
            // Store the original camera position and rotation
            _originalCameraPosition = cameraTarget.transform.position;
            _originalCameraRotation = cameraTarget.transform.rotation.eulerAngles;
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(OverviewLocationEvent eventType)
        {
            if (eventType.LocationActionType == LocationActionType.Approach)

            {
                // Cache current pose as the place to return to
                _returnCameraPosition = cameraTarget.transform.position;
                _returnCameraRotation = cameraTarget.transform.rotation;
                _hasReturnPose = true;

                switch (eventType.LocationType)
                {
                    case LocationType.Trader:
                        SetState(OverviewState.AtTrader, eventType.CameraTransform);
                        break;
                    case LocationType.NpcResidence:
                        SetState(OverviewState.AtNpcResidence, eventType.CameraTransform);
                        break;
                    case LocationType.Laboratory:
                        SetState(OverviewState.AtLaboratory, eventType.CameraTransform);
                        break;
                    case LocationType.MiscNpc:
                        SetState(OverviewState.AtMiscNpc, eventType.CameraTransform);
                        break;
                    default:
                        Debug.LogWarning($"Unhandled LocationType: {eventType.LocationType}");
                        break;
                }
            }


            if (eventType.LocationActionType == LocationActionType.RetreatFrom)
                SetState(OverviewState.Overview);
        }

        public void SetState(OverviewState newState, Transform cameraTransform = null)
        {
            CurrentState = newState;

            if (cameraTransform != null)
                cameraTarget.transform.SetPositionAndRotation(
                    cameraTransform.position, cameraTransform.rotation);


            if (newState == OverviewState.Overview)
            {
                // Prefer the cached return pose from before we entered the sub‑location
                if (_hasReturnPose)
                {
                    cameraTarget.transform.SetPositionAndRotation(_returnCameraPosition, _returnCameraRotation);
                    _hasReturnPose = false; // consume it
                    return;
                }

                // Next best: the current dock's overview anchor
                // var dock = DockManager.Instance?.currentDockInteractable;
                // if (dock?.overviewCameraTarget != null)
                // {
                //     cameraTarget.transform.SetPositionAndRotation(
                //         dock.overviewCameraTarget.position, dock.overviewCameraTarget.rotation);
                //     return;
                // }
                //
                // Final fallback: original snapshot from OnEnable
                cameraTarget.transform.position = _originalCameraPosition;
                cameraTarget.transform.rotation = Quaternion.Euler(_originalCameraRotation);
            }
        }
    }
}