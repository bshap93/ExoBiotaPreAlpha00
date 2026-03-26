using System;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum SpontaneousTriggerEventType
    {
        Triggered,
        Silenced
    }

    public struct SpontaneousTriggerEvent
    {
        static SpontaneousTriggerEvent _e;

        public string UniqueID;
        public SpontaneousTriggerEventType EventType;
        public int IntParameter;
        public string StringParameter;
        public string SecondaryStringParameter;

        public static void Trigger(string uniqueID, SpontaneousTriggerEventType eventType, int intParameter = 0,
            string stringParameter = null, string secondaryStringParameter = null)
        {
            _e.UniqueID = uniqueID;
            _e.EventType = eventType;
            _e.IntParameter = intParameter;
            _e.StringParameter = stringParameter;
            _e.SecondaryStringParameter = secondaryStringParameter;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
