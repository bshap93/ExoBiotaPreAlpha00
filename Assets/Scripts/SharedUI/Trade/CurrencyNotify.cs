using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

namespace SharedUI.Trade
{
    public class CurrencyNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text currencyAmtText;
        [SerializeField] MMFeedbacks addCurrencyFeedback;

        void OnEnable()
        {
            addCurrencyFeedback.PlayFeedbacks();
        }
        public void SetCurrencyText(string currencyAmount)
        {
            currencyAmtText.text = currencyAmount;
        }
    }
}
