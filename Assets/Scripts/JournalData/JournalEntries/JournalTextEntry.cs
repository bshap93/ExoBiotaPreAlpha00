using UnityEngine;

namespace JournalData.JournalEntries
{
    [CreateAssetMenu(fileName = "JournalTextEntry", menuName = "Scriptable Objects/Journal/JournalTextEntry")]
    public class JournalTextEntry : JournalEntry
    {
        [Header("Text Entry")] [TextArea] public string entryTextDescription;
    }
}
