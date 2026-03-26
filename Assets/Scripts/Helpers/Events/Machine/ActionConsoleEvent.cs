using System;
using MoreMountains.Tools;

namespace Helpers.Events.Machine
{
    [Serializable]
    public enum ActionConsoleEventType
    {
        RequestActionConsoleHailsPlayer
    }

    public struct ActionConsoleEvent
    {
        static ActionConsoleEvent _e;

        public string UniqueID;
        public ActionConsoleEventType EventType;


        public static void Trigger(string uniqueID, ActionConsoleEventType eventType)
        {
            _e.UniqueID = uniqueID;
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
