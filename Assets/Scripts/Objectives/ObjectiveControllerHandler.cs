using CompassNavigatorPro;
using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives
{
    public class ObjectiveControllerHandler : MonoBehaviour, MMEventListener<ObjectiveEvent>
    {
        public UnityEvent onObjectiveComplete;
        public UnityEvent onObjectiveActive;
        [SerializeField] string objectiveId;
        [SerializeField] CompassProPOI compassProPOI;


        void Start()
        {
            if (compassProPOI != null)
                compassProPOI.enabled = false;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ObjectiveEvent eventType)
        {
            // Debug.Log($"Handler for '{objectiveId}' received event: {eventType.type} for '{eventType.objectiveId}'");
            // Debug.Log(
            //     $"String comparison: '{objectiveId}' == '{eventType.objectiveId}' = {objectiveId == eventType.objectiveId}");

            if (eventType.objectiveId == objectiveId)
            {
                if (eventType.type == ObjectiveEventType.ObjectiveActivated)
                    HandleSetObjectiveActive();
                else if (eventType.type == ObjectiveEventType.ObjectiveCompleted) HandleSetObjectiveComplete();
            }
        }

        public void HandleSetObjectiveActive()
        {
            onObjectiveActive?.Invoke();
            if (compassProPOI != null) compassProPOI.enabled = true;
        }

        public void HandleSetObjectiveComplete()
        {
            onObjectiveComplete?.Invoke();

            if (compassProPOI != null)
            {
                Debug.Log($"Objective {objectiveId} is now complete. Disabling CompassProPOI: {compassProPOI}");
                compassProPOI.enabled = false;
            }
        }

        public void TriggerCompleteObjective()
        {
            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveCompleted);
        }

        public void TriggerCompleteAllActiveObjectives()
        {
            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.CompleteAllActiveObjectives);
        }

        public void TriggerCompleteAllObjectivesPreviousTo()
        {
            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.CompleteAllObjectivesPreviousTo);
        }
    }
}
