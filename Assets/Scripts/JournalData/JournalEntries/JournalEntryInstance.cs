namespace JournalData.JournalEntries
{
    public enum JournalEntryState
    {
        Unread,
        Read
    }

    public class JournalEntryInstance
    {
        public JournalEntry EntryData;

        public JournalEntryState State;
    }
}
