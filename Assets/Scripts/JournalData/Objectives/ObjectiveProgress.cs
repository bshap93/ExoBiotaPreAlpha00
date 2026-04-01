using System;

namespace Objectives
{
    public enum ObjectiveProgressType
    {
        DoThingNTimes,
        None // Objective has no in between progress
    }

    [Serializable]
    public class ObjectiveProgress
    {
        public int objectiveID;
        public ObjectiveProgressType progressType = ObjectiveProgressType.None;
        public int initialProgress;
        public int currentProgress;
        public int targetProgress = 1;
    }
}
