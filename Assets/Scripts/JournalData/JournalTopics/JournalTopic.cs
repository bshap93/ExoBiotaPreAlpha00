using System;
using JournalData.JournalEntries;
using UnityEngine;
using Utilities.Interface;

namespace JournalData.JournalTopics
{
    [Serializable]
    public enum JournalTopicType
    {
        Character,
        Narrative,
        Location
    }

    [CreateAssetMenu(fileName = "JournalTopic", menuName = "Scriptable Objects/Journal/JournalTopic")]
    public class JournalTopic : ScriptableObject, IRequiresUniqueID
    {
        public string uniqueID;
        public string journalTopicName;
        public JournalEntry[] associatedEntries;
        public JournalTopicType topicType;

        public string[] keywords;
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
        // public JournalTopic Copy()
        // {
        //     
        // }
    }
}
