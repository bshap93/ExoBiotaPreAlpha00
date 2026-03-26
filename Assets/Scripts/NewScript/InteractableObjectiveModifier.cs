using System;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Objectives.ScriptableObjects;
using UnityEngine;

namespace NewScript
{
    public class InteractableObjectiveModifier : MonoBehaviour, IInteractable
    {
        public enum ObjectiveActionType
        {
            Add,
            Activate,
            Complete,
            Deactivate,
            Delete
        }

        [SerializeField] ObjectiveObject objective;
        [SerializeField] ObjectiveActionType objectiveAction;


        public void Interact()
        {
            switch (objectiveAction)
            {
                case ObjectiveActionType.Add:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveAdded);
                    break;
                case ObjectiveActionType.Activate:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveActivated);
                    break;
                case ObjectiveActionType.Complete:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                    break;
                case ObjectiveActionType.Deactivate:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeactivated);
                    break;
                case ObjectiveActionType.Delete:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }
        public void OnInteractionStart()
        {
            throw new NotImplementedException();
        }
        public void OnInteractionEnd(string param)
        {
            throw new NotImplementedException();
        }
        public bool CanInteract()
        {
            throw new NotImplementedException();
        }
        public bool IsInteractable()
        {
            throw new NotImplementedException();
        }
        public void OnFocus()
        {
            throw new NotImplementedException();
        }
        public void OnUnfocus()
        {
            throw new NotImplementedException();
        }
        public float GetInteractionDistance()
        {
            throw new NotImplementedException();
        }
    }
}
