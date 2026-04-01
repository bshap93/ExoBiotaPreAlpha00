using System.Collections.Generic;
using System.Linq;
using Helpers.Events;
using MoreMountains.Tools;
using Objectives;
using Objectives.ScriptableObjects;
using SharedUI.IGUI;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Objectives
{
    public class ObjectivesHUDController : MonoBehaviour, MMEventListener<ObjectiveEvent>,
        MMEventListener<LoadedManagerEvent>, MMEventListener<MyUIEvent>
    {
        [SerializeField] Transform listTransform;
        [SerializeField] GameObject objectiveListItemPrefab;
        [SerializeField] Image scrollAreaImage;
        [SerializeField] Image backgroundForObjective;

        [SerializeField] CanvasGroup subCanvasGroup;

        void OnEnable()
        {
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<ObjectiveEvent>();
            this.MMEventStartListening<MyUIEvent>();
            Refresh();
        }

        void OnDisable()
        {
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<ObjectiveEvent>();
            this.MMEventStopListening<MyUIEvent>();
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All) Initialize();
            Refresh();
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.BreakableInteractChoice)
                if (eventType.uiActionType == UIActionType.Open)
                {
                    // hide objectives HUD when interact choice UI is open
                    subCanvasGroup.alpha = 0;
                    subCanvasGroup.interactable = false;
                    subCanvasGroup.blocksRaycasts = false;
                }
                else if (eventType.uiActionType == UIActionType.Close)
                {
                    // show objectives HUD when interact choice UI is closed
                    subCanvasGroup.alpha = 1;
                    subCanvasGroup.interactable = true;
                    subCanvasGroup.blocksRaycasts = true;
                }
        }


        public void OnMMEvent(ObjectiveEvent e)
        {
            // Any activation/completion should refresh the list if we're visible
            if (e.type == ObjectiveEventType.ObjectiveActivated ||
                e.type == ObjectiveEventType.ObjectiveDeactivated ||
                e.type == ObjectiveEventType.ObjectiveCompleted ||
                e.type == ObjectiveEventType.ObjectiveAdded || e.type == ObjectiveEventType.Refresh ||
                e.type == ObjectiveEventType.IncrementObjectiveProgress)
                Refresh();
            // else if (e.type == ObjectiveEventType.ObjectiveProgressMade)
            // {
            //     // If progress made, only refresh if the objective is already in the list
            //     var mgr = ObjectivesManager.Instance;
            //     if (mgr == null) return;
            //
            //     var objectiveProgress = mgr.GetIntProgressForObjective(e.objectiveId);
            //
            //     
            // }
        }

        void Initialize()
        {
            Refresh();
        }

        public void Refresh()
        {
            var mgr = ObjectivesManager.Instance;
            if (mgr == null) return;

            // Clear existing list
            foreach (Transform child in listTransform)
                Destroy(child.gameObject);

            var enumerator = EnumerateHUDItems(mgr);
            if (!enumerator.Any())
            {
                scrollAreaImage.enabled = false;
                backgroundForObjective.enabled = false;
                return;
            }

            scrollAreaImage.enabled = true;
            backgroundForObjective.enabled = true;


            foreach (var obj in EnumerateHUDItems(mgr))
            {
                if (obj == null) continue;
                var go = Instantiate(objectiveListItemPrefab, listTransform);
                var element = go.GetComponent<ObjectiveHUDListElement>();
                var objectiveProgress = 0;
                if (obj.objectiveProgressType == ObjectiveProgressType.DoThingNTimes)
                    objectiveProgress = mgr.GetIntProgressForObjective(obj.objectiveId);

                element.Initialize(obj, objectiveProgress);
            }
        }

        static IEnumerable<ObjectiveObject> EnumerateHUDItems(ObjectivesManager mgr)
        {
            foreach (var id in mgr.GetActiveObjectives())
                yield return mgr.GetObjectiveById(id);
        }
    }
}
