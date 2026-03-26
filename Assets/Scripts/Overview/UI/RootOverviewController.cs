using System;
using System.Collections.Generic;
using Dirigible.Interactable;
using Dirigible.Interface;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.UI;
using FirstPersonPlayer.UI.LocationButtonBase;
using Helpers.Events;
using MoreMountains.Tools;
using Objectives;
using UnityEngine;
using UnityEngine.Serialization;

namespace Overview.UI
{
    public class RootOverviewController : MonoBehaviour, ICanvasGroupController, MMEventListener<ObjectiveEvent>
    {
        [FormerlySerializedAs("_dirigibleDockInteractable")]
        readonly List<GameObject> hotspotObjects = new();


        CanvasGroup _canvasGroup;

        public static RootOverviewController Instance { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null) Debug.LogWarning("CanvasGroup component not found on RootOverviewController.");
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }


        public void HideCanvasGroup()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                Debug.LogWarning("CanvasGroup is not set, cannot hide.");
            }
        }

        public void ShowCanvasGroup()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogWarning("CanvasGroup is not set, cannot show.");
            }
        }

        public void OnMMEvent(ObjectiveEvent eventType)
        {
            if (eventType.type == ObjectiveEventType.ObjectiveActivated ||
                eventType.type == ObjectiveEventType.ObjectiveCompleted ||
                eventType.type == ObjectiveEventType.Refresh || eventType.type == ObjectiveEventType.ObjectiveAdded
               )
                EmphasizeLocationsRelevantToActiveObjectives();
        }

        public bool EmphasizeLocationsRelevantToActiveObjectives()
        {
            // if (dirigibleDockInteractable == null || dirigibleDockInteractable.dockLocationHotspotLookupEntries == null)
            // {
            //     Debug.LogWarning("DirigibleDockInteractable or its dockLocationHotspotLookupEntries is not set.");
            //     return false;
            // }

            // foreach (var dockLocation in dirigibleDockInteractable.dockLocationHotspotLookupEntries)
            // {
            //     var activeObjectives = ObjectivesManager.Instance.GetActiveObjectives();
            //     var associatedObjectives = dockLocation.LocationDefinition.GetAssociatedObjectivesSet();
            //
            //     HashSet<string> intersectingObjectives = new(activeObjectives);
            //     if (intersectingObjectives == null) throw new ArgumentNullException(nameof(intersectingObjectives));
            //
            //     intersectingObjectives.IntersectWith(associatedObjectives);
            //
            //     var locationButtons =
            //         dockLocation.hotspotRectTransform.GetComponentInChildren<OverviewModeLocationButtons>();
            //
            //
            //     if (locationButtons == null)
            //         // Debug.LogError(
            //         //     "OverviewModeLocationButtons component is missing on the instantiated hotspot object.");
            //         continue;
            //
            //     if (intersectingObjectives.Count > 0)
            //         locationButtons.SetEmphasized(true);
            //     else
            //         locationButtons.SetEmphasized(false);
            // }

            return true;
        }


        public void ShowOverview()
        {
            if (_canvasGroup != null)
            {
                ShowCanvasGroup();
            }
            else
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup != null)
                    ShowCanvasGroup();
                else
                    Debug.LogWarning("CanvasGroup component not found on RootOverviewController.");
            }

            hotspotObjects.Clear();

            // foreach (var dockLocation in dirigibleDockInteractable.dockLocationHotspotLookupEntries)
            // // {
            // //     // var hotspotObject = Instantiate(
            // //     //     dockLocation.LocationDefinition.prefab,
            // //     //     dockLocation.hotspotRectTransform);
            // //
            // //     // hotspotObjects.Add(hotspotObject);
            // //     // var interactable = hotspotObject.GetComponent<IInteractable>();
            // //     // var locationButtons = hotspotObject.GetComponent<OverviewModeLocationButtons>();
            // //     // if (locationButtons == null)
            // //     // {
            // //     //     Debug.LogError(
            // //     //         "OverviewModeLocationButtons component is missing on the instantiated hotspot object.");
            // //     //
            // //     //     continue;
            // //     // }
            // //
            // //
            // //     var spawnPointId = dockLocation.LocationDefinition.spawnPointId;
            // //     if (string.IsNullOrEmpty(spawnPointId)) continue;
            // //
            // //     var sceneName = dockLocation.LocationDefinition.spawnPointSceneName;
            // //     if (string.IsNullOrEmpty(sceneName))
            // //     {
            // //         Debug.LogError($"Scene name is not set for dock location: {dockLocation.LocationDefinition.name}");
            // //         continue;
            // //     }
            // //
            // //     KeyItemObject keyItemObject;
            // //     if (dockLocation.LocationDefinition.isUnlockingKeyItem)
            // //         keyItemObject = dockLocation.LocationDefinition.keyItemToUnlock;
            // //     else keyItemObject = null;
            // //
            // //     // if (interactable is MineOverviewModeLocation mineLocation)
            // //     //     mineLocation.Initialize(
            // //     //         spawnPointId, sceneName, dockLocation.LocationDefinition.GetMineName(), keyItemObject);
            // // }

            EmphasizeLocationsRelevantToActiveObjectives();
        }

        public void HideOverview()
        {
            // HideCanvasGroup();
            foreach (var hotspotObject in hotspotObjects)
                if (hotspotObject != null)
                    Destroy(hotspotObject);

            hotspotObjects.Clear();
        }
    }
}
