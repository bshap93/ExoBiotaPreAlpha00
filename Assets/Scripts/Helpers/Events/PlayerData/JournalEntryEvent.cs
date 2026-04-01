using MoreMountains.Tools;

namespace Helpers.Events.PlayerData
{
    public enum JournalEntryEventType
    {
        Added,
        MarkedAsRead
    }

    public struct JournalEntryEvent
    {
        static JournalEntryEvent _e;
        public string JournalEntryUniqueId;
        public JournalEntryEventType EventType;

        public static void Trigger(JournalEntryEventType eventType, string journalEntryUniqueId)
        {
            _e.EventType = eventType;
            _e.JournalEntryUniqueId = journalEntryUniqueId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
