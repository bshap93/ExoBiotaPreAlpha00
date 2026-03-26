using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum RespawnEventReason
    {
        Death
    }

    public struct RespawnEvent
    {
        private static RespawnEvent _e;

        public RespawnEventReason Reason;

        public static void Trigger(RespawnEventReason reason)
        {
            _e.Reason = reason;

            MMEventManager.TriggerEvent(_e);
        }
    }
}