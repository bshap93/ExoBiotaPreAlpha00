using System;
using UnityEngine;

namespace JournalData.JournalEntries
{
    [Serializable]
    public enum JournalTextEntryType
    {
        RegularNoteEntry
    }

    [CreateAssetMenu(fileName = "JournalTextEntry", menuName = "Scriptable Objects/Journal/JournalTextEntry")]
    public class JournalTextEntry : JournalEntry
    {
        [Header("Text Entry")] [TextArea] public string entryTextDescription;
        public Color nameTextColor;
        public Color descriptionTextColor;
        public JournalTextEntryType entryType = JournalTextEntryType.RegularNoteEntry;
    }
}
