using System;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine.Serialization;

namespace Helpers.Events
{
    [Serializable]
    public enum ObjectiveEventType
    {
        ObjectiveCompleted,
        ObjectiveActivated,
        CompleteAllActiveObjectives,
        CompleteAllObjectivesPreviousTo,
        ObjectiveDeactivated,
        ObjectiveAdded,
        Refresh,
        IncrementObjectiveProgress,
        ObjectiveDeleted
    }

    public enum NotifyType
    {
        Silent,
        Regular
    }

    [Serializable]
    public struct ObjectiveEvent
    {
        static ObjectiveEvent _e;

        public string objectiveId;
        public ObjectiveEventType type;

        public NotifyType notifyType;

        [FormerlySerializedAs("progressMade")] public int progressMadeByN;
        public float progressMadeByF;
        public string miscProgressInfo;
        public SubjectLocationObject locationObject;


        public static void Trigger(string objectiveId, ObjectiveEventType type,
            NotifyType notifyType = NotifyType.Regular, int progressMade = 0,
            float progressMadeByF = 0f, string miscProgressInfo = null, SubjectLocationObject locationObject = null)
        {
            _e.objectiveId = objectiveId;
            _e.type = type;
            _e.notifyType = notifyType;
            _e.progressMadeByN = progressMade;
            _e.progressMadeByF = progressMadeByF;
            _e.miscProgressInfo = miscProgressInfo;
            _e.locationObject = locationObject;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
