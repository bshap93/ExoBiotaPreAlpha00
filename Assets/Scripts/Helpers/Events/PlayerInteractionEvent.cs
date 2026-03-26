using System;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum PlayerInteractionEventType
    {
        None,
        Interacted
    }

    public struct PlayerInteractionEvent
    {
        static PlayerInteractionEvent _e;

        public PlayerInteractionEventType EventType;

        public static void Trigger(PlayerInteractionEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
