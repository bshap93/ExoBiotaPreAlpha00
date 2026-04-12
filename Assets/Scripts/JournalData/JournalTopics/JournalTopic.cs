using System;
using JournalData.JournalEntries;
using UnityEditor;
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

    public enum ExobiotaArea
    {
        AshpoolSprings,
        MagniumMines
    }

    [CreateAssetMenu(fileName = "JournalTopic", menuName = "Scriptable Objects/Journal/JournalTopic")]
    public class JournalTopic : ScriptableObject, IRequiresUniqueID
    {
        public string uniqueID;
        public string journalTopicName;
        public JournalEntry[] associatedEntries;
        public JournalTopicType topicType;
        public ExobiotaArea associatedArea;

        public string[] keywords;
        // public JournalTopic Copy()
        // {
        //     
        // }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (uniqueID != name)
            {
                uniqueID = name;
                EditorUtility.SetDirty(this);
            }
        }
#endif
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
    }
}
