using Manager.StateManager;
using MoreMountains.Tools;

namespace Helpers.Events.Journal
{
    public enum JournalEntryProviderStateEventType
    {
        SetNewJournalEntryProviderState
    }

    public struct JournalEntryProviderStateEvent
    {
        static JournalEntryProviderStateEvent _e;

        public string UniqueID;
        public JournalEntryProviderStateEventType EventType;
        public JournalEntryProviderManager.EntryProviderState JournalEntryProviderState;

        public static void Trigger(JournalEntryProviderStateEventType eventType, string uniqueID,
            JournalEntryProviderManager.EntryProviderState journalEntryProviderState)
        {
            _e.EventType = eventType;
            _e.UniqueID = uniqueID;
            _e.JournalEntryProviderState = journalEntryProviderState;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
