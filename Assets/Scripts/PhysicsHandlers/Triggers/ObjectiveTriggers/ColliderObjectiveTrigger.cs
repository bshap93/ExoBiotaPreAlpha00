using Helpers.Events;
using Helpers.Events.Triggering;
using Manager;
using Objectives.ScriptableObjects;
using UnityEngine;

namespace PhysicsHandlers.Triggers
{
    public class ColliderObjectiveTrigger : MonoBehaviour
    {
        public bool setNotTriggerableOnExit;
        public bool setNotTriggerableOnEnter;

        [SerializeField] ObjectiveObject objective;

        public string uniqueID;

        // [SerializeField] TriggerType triggerType = TriggerType.OnEnter;
        [SerializeField] ObjectiveAction[] actions;

        TriggerColliderManager _triggerColliderManager;

        void Start()
        {
            _triggerColliderManager = TriggerColliderManager.Instance;
            if (_triggerColliderManager == null)
                Debug.LogWarning("ColliderObjectiveTrigger: No Collider Manager found.", this);
        }

        void OnTriggerEnter(Collider other)
        {
            // if (triggerType != TriggerType.OnEnter) return;
            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                return;

            if (objective == null)
            {
                Debug.LogWarning("ColliderObjectiveTrigger: No objective assigned.", this);
                return;
            }

            if (_triggerColliderManager != null &&
                !_triggerColliderManager.IsObjectiveColliderTriggerable(uniqueID)) return;

            var objectiveId = objective.objectiveId;

            if (setNotTriggerableOnEnter)
                TriggerColliderEvent.Trigger(
                    uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Objective);

            foreach (var action in actions)
                switch (action)
                {
                    case ObjectiveAction.Add:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveAdded);
                        break;
                    case ObjectiveAction.Activate:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveActivated);
                        break;
                    case ObjectiveAction.Complete:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveCompleted);
                        break;
                    case ObjectiveAction.MakeInactive:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveDeactivated);
                        break;
                }
        }

        void OnTriggerExit(Collider other)
        {
            // if (triggerType != TriggerType.OnExit) return;
            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                return;

            if (setNotTriggerableOnExit)
                TriggerColliderEvent.Trigger(
                    uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Objective);
        }

        enum ObjectiveAction
        {
            Add,
            Activate,
            Complete,
            MakeInactive
        }
    }
}
