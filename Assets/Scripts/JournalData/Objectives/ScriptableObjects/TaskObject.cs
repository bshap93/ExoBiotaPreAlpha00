using System;
using UnityEngine;

namespace Objectives.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(
        fileName = "TaskObject", menuName = "Scriptable Objects/Objectives/TaskObject",
        order = 1)]
    public class TaskObject : ScriptableObject
    {
        public string parentObjectiveId;

        public string taskText;

        public bool shouldBeMadeActiveOnAdd;

        public bool autoAddWhenPrerequisitesMet;


        public string[] addWhenTasksCompleted;

        public ObjectiveObject GetParentObjective()
        {
            foreach (var obj in ObjectivesManager.Instance.Objectives)
                if (obj.objectiveId == parentObjectiveId)
                    return obj;

            Debug.LogWarning($"Parent Objective with ID {parentObjectiveId} not found.");
            return null;
        }
    }
}
