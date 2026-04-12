using System;
using JournalData.JournalTopics;
using UnityEditor;
using UnityEngine;
using Utilities.Interface;

namespace JournalData.JournalEntries
{
    [Serializable]
    public enum JournalEntryType
    {
        BasicTextEntry,
        ImageEntryWithText
    }

    [Serializable]
    public abstract class JournalEntry : ScriptableObject, IRequiresUniqueID
    {
        public string uniqueID;
        [Header("References")] public JournalTopic parentalTopic;

        [Header("Entry Data")] public string entryName;
        public Sprite optionalEntryIcon;

        public string[] keywords;
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
