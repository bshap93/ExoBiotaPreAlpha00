using System;
using Helpers.Events;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Objectives.ActionTasks
{
    [Category("Objectives")]
    [Description("Use to increment a multi-part or complete a single part objective.")]
    public class OperateObjective : ActionTask
    {
        [Serializable]
        public enum OperationType
        {
            Increment,
            Complete,
            Add,
            Activate,
            Deactivate,
            Remove
        }

        public readonly BBParameter<string> ObjectiveID;
        public readonly BBParameter<OperationType> OperationTypeParam;
        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            return null;
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            if (ObjectiveID.value == null)
            {
                EndAction(true);
                return;
            }

            switch (OperationTypeParam.value)
            {
                case OperationType.Increment:
                    ObjectiveEvent.Trigger(
                        ObjectiveID.value, ObjectiveEventType.IncrementObjectiveProgress, NotifyType.Regular, 1);

                    break;
                case OperationType.Complete:
                    ObjectiveEvent.Trigger(
                        ObjectiveID.value, ObjectiveEventType.ObjectiveCompleted);

                    break;
                case OperationType.Add:
                    ObjectiveEvent.Trigger(
                        ObjectiveID.value, ObjectiveEventType.ObjectiveAdded);

                    break;
                case OperationType.Activate:
                    ObjectiveEvent.Trigger(
                        ObjectiveID.value, ObjectiveEventType.ObjectiveActivated);

                    break;
                case OperationType.Deactivate:
                    ObjectiveEvent.Trigger(
                        ObjectiveID.value, ObjectiveEventType.ObjectiveDeactivated);

                    break;
                case OperationType.Remove:
                    ObjectiveEvent.Trigger(
                        ObjectiveID.value, ObjectiveEventType.ObjectiveDeleted);

                    break;
            }

            EndAction(true);
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
        }

        //Called when the task is disabled.
        protected override void OnStop()
        {
        }

        //Called when the task is paused.
        protected override void OnPause()
        {
        }
    }
}
