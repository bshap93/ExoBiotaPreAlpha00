using Helpers.Events;
using MoreMountains.Feedbacks;
using Objectives.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.CustomFeedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to trigger an Objective Action when played.")]
    [FeedbackPath("Objectives/Objective Action")]
    public class MMF_ObjectiveAction : MMF_Feedback
    {
        public enum ObjectiveAction
        {
            AddObjective,
            CompleteObjective,
            IncrementObjective
        }

        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        [MMFInspectorGroup("Objective Action", true, 45)] [Tooltip("The action to perform on the objective.")]
        public ObjectiveAction objectiveAction;
        [FormerlySerializedAs("ObjectiveObject")] [Tooltip("The Objective Object to perform the action on.")]
        public ObjectiveObject objectiveObject;
        public int progressToAdd = 1;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active || !FeedbackTypeAuthorized || objectiveObject == null) return;

            switch (objectiveAction)
            {
                case ObjectiveAction.AddObjective:
                    ObjectiveEvent.Trigger(objectiveObject.objectiveId, ObjectiveEventType.ObjectiveAdded);
                    break;
                case ObjectiveAction.CompleteObjective:
                    ObjectiveEvent.Trigger(objectiveObject.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                    break;
                case ObjectiveAction.IncrementObjective:
                    ObjectiveEvent.Trigger(
                        objectiveObject.objectiveId, ObjectiveEventType.IncrementObjectiveProgress,
                        progressMade: progressToAdd);

                    break;
                default:
                    Debug.LogWarning("Unknown Objective Action");
                    break;
            }
        }
    }
}
