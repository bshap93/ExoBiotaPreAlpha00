using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI.Alert
{
    public class PauseAndGiveInfoPanel : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [FormerlySerializedAs("Title")] [SerializeField]
        public TMP_Text title;
        [SerializeField] Image descriptorImage;
        [SerializeField] TMP_Text shortDescriptionText;
        [SerializeField] TMP_Text longDescriptionText;
        [SerializeField] public ButtonManager continueButton;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }


        public void Initialize(PauseAndGiveInfoDetails details)
        {
            // Initialization code here
            title.text = details.title;
            descriptorImage.sprite = details.descriptorImage;
            shortDescriptionText.text = details.shortDescription;
            longDescriptionText.text = details.longDescription;
        }
        public void Open()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        public void Close()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
