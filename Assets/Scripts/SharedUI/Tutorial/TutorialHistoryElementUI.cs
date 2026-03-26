using Helpers.Events.Tutorial;
using Michsky.MUIP;
using TMPro;
using UnityEngine;

namespace SharedUI.Tutorial
{
    public class TutorialHistoryElementUI : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] ButtonManager infoButton;

        public void Initialize(string tutBitId)
        {
            nameText.text = tutBitId;
            infoButton.onClick.AddListener(() =>
            {
                MainTutorialBitEvent.Trigger(tutBitId, MainTutorialBitEventType.ShowMainTutBit);
            });
        }
    }
}
