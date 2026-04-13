using System.Globalization;
using Manager;
using Manager.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.IGUI
{
    public class TradeResourcesIGUI : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        PlayerCurrencyManager playerCurrencyManager;


        [Header("UI elements")] [SerializeField]
        TMP_Text primaryCurrencyAmountText;
        [SerializeField] TMP_Text primaryCurrencyUnitsText;
        [SerializeField] Image resourceTypeIcon;
        [SerializeField] TMP_Text resourceAmtText;
        [SerializeField] TMP_Text resourceUnitsText;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
        void OnEnable()
        {
            SetValues();
        }

        void SetValues()
        {
            if (playerCurrencyManager == null) return;

            primaryCurrencyAmountText.text =
                playerCurrencyManager.PlayerPrimaryCurrencyAmount.ToString(CultureInfo.InvariantCulture);

            primaryCurrencyUnitsText.text = "P";

            resourceTypeIcon.sprite = ExaminationManager.Instance.iconRepository.scrapIcon;
            resourceAmtText.text =
                playerCurrencyManager.PlayerSecondaryCurrencyAmount.ToString(CultureInfo.InvariantCulture);

            resourceUnitsText.text = "KG";
        }
    }
}
