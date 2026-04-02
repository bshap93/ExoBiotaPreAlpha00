using TMPro;
using UnityEngine;

namespace SharedUI.Progression
{
    public class JournalNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text journalEntityName;

        public void SetJournalEntityText(string text)
        {
            journalEntityName.text = text;
        }
    }
}
