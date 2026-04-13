using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.Trade
{
    public class PrimaryCurrencyNotify : MonoBehaviour
    {
        [FormerlySerializedAs("currencyAmtText")] [SerializeField]
        TMP_Text resourceAmtText;

        public void SetPrimaryCurrencyAmountText(string currencyAmount)
        {
            resourceAmtText.text = currencyAmount;
        }
    }
}
