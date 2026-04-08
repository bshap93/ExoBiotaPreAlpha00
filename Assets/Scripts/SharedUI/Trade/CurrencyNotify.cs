using TMPro;
using UnityEngine;

namespace SharedUI.Trade
{
    public class CurrencyNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text currencyAmtText;
        public void SetCurrencyText(string currencyAmount)
        {
            currencyAmtText.text = currencyAmount;
        }
    }
}
