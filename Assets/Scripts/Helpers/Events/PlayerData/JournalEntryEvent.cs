using MoreMountains.Tools;

namespace Helpers.Events.PlayerData
{
    public struct JournalEntryEvent
    {
        static JournalEntryEvent _e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(_e);
        }
    }
}
