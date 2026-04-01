using System;

namespace JournalData.JournalEntries
{
    [Serializable]
    public enum JournalEntryState
    {
        Unread,
        Read
    }

    [Serializable]
    public class JournalEntryInstance
    {
        public string entryID;

        public JournalEntryState state;

        public DateTime AquiredAt;
    }
}
