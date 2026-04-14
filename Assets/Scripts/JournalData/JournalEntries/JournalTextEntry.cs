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
        public Color nameTextColor = new(238 / 255f, 242 / 255f, 187 / 255f);
        public Color descriptionTextColor = new(238 / 255f, 242 / 255f, 187 / 255f);
        public JournalTextEntryType entryType = JournalTextEntryType.RegularNoteEntry;
    }
}
