using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace JournalData.JournalTopics
{
    [Serializable]
    public class JournalTopicInstance
    {
        [FormerlySerializedAs("aquiredJournalEntries")]
        public List<string> aquiredJournalEntryUniqueIds;
        public string journalTopicUniqueId;
        public DateTime aquiredAt;
    }
}
