using MoreMountains.Tools;

namespace Helpers.Events.UpdateUI
{
    public struct UpdateInventoryWindowEvent
    {
        static UpdateInventoryWindowEvent _e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(_e);
        }
    }
}
