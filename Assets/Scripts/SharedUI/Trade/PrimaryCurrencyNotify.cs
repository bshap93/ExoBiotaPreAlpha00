using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.Trade
{
    public class PrimaryCurrencyNotify : MonoBehaviour
    {
        [FormerlySerializedAs("currencyAmtText")] [SerializeField]
        TMP_Text resourceAmtText;
        [FormerlySerializedAs("addCurrencyFeedback")] [SerializeField]
        MMFeedbacks addResourceFeedback;

        void OnEnable()
        {
            addResourceFeedback.PlayFeedbacks();
        }
        public void SetPrimaryCurrencyAmountText(string currencyAmount)
        {
            resourceAmtText.text = currencyAmount;
        }
    }
}
