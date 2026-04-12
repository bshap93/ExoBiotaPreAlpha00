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
        public Color nameTextColor = new(238, 242, 187);
        public Color descriptionTextColor = new(238, 242, 187);
        public JournalTextEntryType entryType = JournalTextEntryType.RegularNoteEntry;
    }
}
