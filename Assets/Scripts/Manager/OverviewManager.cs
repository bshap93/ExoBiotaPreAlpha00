// using Dirigible.Interactable;
// using Events;
// using MoreMountains.Tools;
// using Overview.OverviewMode.ScriptableObjectDefinitions;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Manager
// {
//     public class OverviewManager : MonoBehaviour, MMEventListener<DockingEvent>
//     {
//         [FormerlySerializedAs("mostRecentDockDefinition")]
//         public DockDefinition currentDock;
//
//         public string sceneName;
//
//         public DirigibleDockInteractable[] dirigibleDocksInCurrentScene;
//
//         public void OnEnable()
//         {
//             this.MMEventStartListening();
//         }
//
//         public void OnDisable()
//         {
//             this.MMEventStopListening();
//         }
//
//         public void OnMMEvent(DockingEvent eventType)
//         {
//             if (eventType.EventType == DockingEventType.SetCurrentDock)
//             {
//                 Debug.Log(eventType.ToString());
//                 currentDock = eventType.DockDefinition;
//             }
//
//             if (eventType.EventType == DockingEventType.UnsetCurrentDock)
//             {
//                 if (currentDock == null)
//                 {
//                     // We already Unset the current dock, so we can just return
//                     Debug.LogWarning(
//                         $"Attempted to unset dock {eventType.DockDefinition.dockId}, but current dock is already null");
//                     return;
//                 }
//
//                 // If the current dock is the one being left, then it should be unset
//                 // If it isn't, then we have overlapping dock trigger colliders, and we should log a warning
//                 if (currentDock.dockId == eventType.DockDefinition.dockId)
//                     currentDock = null;
//                 else
//                     Debug.LogWarning(
//                         $"Attempted to unset dock {eventType.DockDefinition.dockId}, but current dock is {currentDock?.dockId}");
//             }
//         }
//
//
//         public DirigibleDockInteractable GetCurrentDockObject()
//         {
//             if (currentDock == null) return null; // ← guard
//
//             if (dirigibleDocksInCurrentScene != null && dirigibleDocksInCurrentScene.Length > 0)
//                 foreach (var dock in dirigibleDocksInCurrentScene)
//                     if (dock != null && dock.def != null && dock.def.dockId == currentDock.dockId)
//                         return dock;
//
//             return null;
//         }
//
//         public void TriggerGetDocksInScene()
//         {
//             dirigibleDocksInCurrentScene = FindObjectsByType<DirigibleDockInteractable>(FindObjectsSortMode.None);
//         }
//
//         public DirigibleDockInteractable GetDockInCurrentScne(DockDefinition eventTypeDockDefinition)
//         {
//             if (dirigibleDocksInCurrentScene != null && dirigibleDocksInCurrentScene.Length > 0)
//                 foreach (var dock in dirigibleDocksInCurrentScene)
//                     if (dock.def.dockId == eventTypeDockDefinition.dockId)
//                         return dock;
//
//             Debug.LogWarning($"No dock found for id: {eventTypeDockDefinition.dockId}");
//             return null;
//         }
//     }
// }

