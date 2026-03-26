using System;
using UnityEngine;

namespace Objectives.ScriptableObjects
{
    public enum ObjectiveStatus
    {
        Completed,
        Active,
        Added,
        NotAdded
    }

    [Serializable]
    public class DialogueCondition
    {
        [SerializeField] public string conditionName; // For inspector clarity
        [SerializeField] public string objectiveId;
        [SerializeField] public ObjectiveStatus requiredStatus;
        [SerializeField] public string startNode;

        public bool CheckCondition(ObjectivesManager manager)
        {
            switch (requiredStatus)
            {
                case ObjectiveStatus.Completed:
                    return manager.IsObjectiveCompleted(objectiveId);
                case ObjectiveStatus.Active:
                    return manager.IsObjectiveActive(objectiveId);
                case ObjectiveStatus.Added:
                    return manager.IsObjectiveAdded(objectiveId);
                case ObjectiveStatus.NotAdded:
                    return !manager.IsObjectiveAdded(objectiveId);
                default:
                    return false;
            }
        }
    }
}
