using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct PlayerStatsSyncEvent
    {
        public static void Trigger()
        {
            MMEventManager.TriggerEvent(new PlayerStatsSyncEvent());
        }
    }
}