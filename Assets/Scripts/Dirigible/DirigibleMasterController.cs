// using System;
// using System.Collections;
// using Domains.Gameplay.Managers.Scripts;
// using Domains.Gameplay.Managers.Scripts.Event;
// using MoreMountains.Tools;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Domains.Gameplay.DirigibleFlight
// {
//     public enum DirigibleState
//     {
//         Stripped,
//         AssembledStarter
//     }
//
//     public class DirigibleMasterController : MonoBehaviour//, MMEventListener<GameStateEvent>
//     {
//         public GameObject systems;
//         public GameObject topHalf;
//         public GameObject cabin;
//
//         [SerializeField] private Rigidbody rb;
//
//         [SerializeField] private DirigibleInput dirigibleInput;
//         [SerializeField] private DirigibleMovementController dirigibleMovementController;
//
//         [FormerlySerializedAs("dirigibleEffectController")] [SerializeField]
//         private DirigibleAbilityController dirigibleAbilityController;
//
//         [SerializeField] private DirigibleEffectsController dirigibleEffectsController;
//
//         // [SerializeField] private DirigibleCameraController dirigibleCameraController;
//         [SerializeField] private DockedCameraController dockedCameraController;
//
//         public DirigibleState dirigibleState = DirigibleState.Stripped;
//
//         [SerializeField] private GameObject mainDirigibleCamera;
//
//         [SerializeField] private bool isPlayerStartingInDirigible;
//
//         [SerializeField] private GameObject dockingGear;
//
//         [SerializeField] public DirigibleDockInteractable currentDock;
//
//         private bool _isDocking;
//
//         private bool _isPlayerInDirigible;
//
//         private void Awake()
//         {
//             // Initialize the dirigible state
//             InitializeDirigible();
//
//             if (isPlayerStartingInDirigible)
//                 EnableControlComponents();
//             else
//                 DisableControlComponents();
//         }
//
//         private void OnEnable()
//         {
//             this.MMEventStartListening();
//         }
//
//         private void OnDisable()
//         {
//             this.MMEventStopListening();
//         }
//
//         public void OnMMEvent(GameStateEvent eventType)
//         {
//             if (eventType.EventType == GameStateEventType.Switch)
//             {
//                 if (eventType.TargetStateType == GameStateManager.GameStateType.Dirigible)
//                 {
//                     EnableControlComponents();
//                     dockingGear.SetActive(false);
//                     _isPlayerInDirigible = true;
//                 }
//                 else
//                 {
//                     DisableControlComponents();
//                     dockingGear.SetActive(true);
//                     _isPlayerInDirigible = false;
//                 }
//             }
//         }
//
//         public bool IsPlayerInDirigible()
//         {
//             return _isPlayerInDirigible;
//         }
//
//         private void InitializeDirigible()
//         {
//             if (systems != null)
//                 switch (dirigibleState)
//                 {
//                     case DirigibleState.Stripped:
//                         InitializeStripped();
//                         break;
//                     case DirigibleState.AssembledStarter:
//                         InitializeStarterAssembled();
//                         break;
//
//                     default:
//                         throw new ArgumentOutOfRangeException();
//                 }
//         }
//
//         private void InitializeStarterAssembled()
//         {
//             systems.SetActive(true);
//             topHalf.SetActive(true);
//             cabin.SetActive(true);
//         }
//
//         private void EnableControlComponents()
//         {
//             mainDirigibleCamera.SetActive(true); // Enable the main camera 
//             dirigibleInput.enabled = true;
//             dirigibleMovementController.enabled = true;
//             dirigibleAbilityController.enabled = true;
//             rb.isKinematic = false; // Allow movement when control is enabled
//             // mainDirigibleCamera.SetActive(true); // Enable the main camera
//         }
//
//         private void DisableControlComponents()
//         {
//             mainDirigibleCamera.SetActive(false); // Disable the main camera
//             dirigibleInput.enabled = false;
//             dirigibleMovementController.enabled = false;
//             dirigibleAbilityController.enabled = false;
//             rb.isKinematic = true; // Prevent movement when control is disabled
//         }
//
//
//         private void InitializeStripped()
//         {
//             systems.SetActive(false);
//             topHalf.SetActive(false);
//             cabin.SetActive(true);
//
//
//             if (isPlayerStartingInDirigible)
//                 UnityEngine.Debug.LogWarning("Player cannot start in a stripped dirigible.");
//         }
//
//
//         public void DockAt(Transform dockTransform, DirigibleDockInteractable.DockType dockType,
//             Transform dockedCameraAnchor)
//         {
//             if (_isDocking) return;
//             _isDocking = true;
//             // StartCoroutine(SmoothDock(dockTransform));
//
//
//             transform.position = dockTransform.position;
//             transform.rotation = dockTransform.rotation;
//
//             DisableControlComponents();
//             rb.isKinematic = true;
//
//             GameStateEvent.Trigger(GameStateEventType.Switch, GameStateManager.GameStateType.DockedDirigible,
//                 dockTransform);
//
//
//             if (dockedCameraController != null) dockedCameraController.SetDockTarget(dockedCameraAnchor);
//
//
//             _isDocking = false;
//         }
//
//         public void Undock()
//         {
//             var cameraController = FindObjectOfType<DockedCameraController>();
//             if (cameraController != null) cameraController.SetDockTarget(null); // stops following dock anchor
//
//             GameStateEvent.Trigger(GameStateEventType.Switch, GameStateManager.GameStateType.Dirigible, null);
//         }
//
//         private IEnumerator SmoothDock(Transform dockTransform, Transform playerSpawnPoint)
//         {
//             const float duration = 1f;
//             var elapsed = 0f;
//
//             var startPos = transform.position;
//             var startRot = transform.rotation;
//
//             // Disable input to prevent interference
//             DisableControlComponents();
//             rb.isKinematic = true;
//
//             while (elapsed < duration)
//             {
//                 var t = elapsed / duration;
//                 transform.position = Vector3.Lerp(startPos, dockTransform.position, t);
//                 transform.rotation = Quaternion.Slerp(startRot, dockTransform.rotation, t);
//                 elapsed += Time.deltaTime;
//                 yield return null;
//             }
//
//             transform.position = dockTransform.position;
//             transform.rotation = dockTransform.rotation;
//
//             // Switch to First Person after docking completes
//             GameStateEvent.Trigger(GameStateEventType.Switch, GameStateManager.GameStateType.FirstPerson,
//                 playerSpawnPoint);
//             _isDocking = false;
//         }
//
//         public void OnExitToFirstPerson()
//         {
//             _isPlayerInDirigible = false;
//
//             // Disable components manually instead of full GameObject
//             if (dirigibleInput != null) dirigibleInput.enabled = false;
//             if (dirigibleMovementController != null) dirigibleMovementController.enabled = false;
//             if (dirigibleAbilityController != null) dirigibleAbilityController.enabled = false;
//
//             if (rb != null) rb.isKinematic = true;
//
//             if (dockingGear != null) dockingGear.SetActive(true);
//         }
//     }
// }

