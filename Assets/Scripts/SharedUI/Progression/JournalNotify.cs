using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.Progression
{
    public class JournalNotify : MonoBehaviour
    {
        [FormerlySerializedAs("journalEntityName")] [SerializeField]
        TMP_Text journalEntityText;

        public void SetJournalEntityText(string text)
        {
            journalEntityText.text = text;
        }
    }
}
