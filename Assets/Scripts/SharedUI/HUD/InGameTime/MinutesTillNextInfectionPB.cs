using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.HUD.InGameTime
{
    public class MinutesTillNextInfectionPb : MonoBehaviour
    {
        public string unitExtension;
        public Image fillImage;
        public TMP_Text valueText;


        public void UpdateUI(float value, float maxVal)
        {
            fillImage.fillAmount = 1 - value / maxVal;
            valueText.text = $"{value:0}{unitExtension}";
        }
    }
}
