using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Inventory
{
    public class GradeCoresUILVRow : MonoBehaviour
    {
        [SerializeField] Image coreImage;
        [SerializeField] TMP_Text coreNameText;
        [SerializeField] TMP_Text coreQuantityText;

        [SerializeField] MMFeedbacks convertToXPFeedback;

        public ButtonManager convertToXPButton;
        [SerializeField] OuterCoreItemObject.CoreObjectValueGrade _coreGrade;


        int _currentQuantity;


        public void Initialize(OuterCoreItemObject.CoreObjectValueGrade grade, int quantity)
        {
            _currentQuantity = quantity;
            coreQuantityText.text = _currentQuantity.ToString();

            if (convertToXPButton != null)
            {
                convertToXPButton.onClick.RemoveAllListeners();
                convertToXPButton.onClick.AddListener(ConvertToXP);
            }
        }

        void ConvertToXP()
        {
            convertToXPFeedback?.PlayFeedbacks();
            BioticCoreXPConversionEvent.Trigger(BioticCoreXPEventType.ConvertCoreToXP, _coreGrade);
        }
    }
}
