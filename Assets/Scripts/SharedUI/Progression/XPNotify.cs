using TMPro;
using UnityEngine;

namespace SharedUI.Progression
{
    public class XPNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text newXPText;
        public void SetXPText(int xpAmount)
        {
            newXPText.text = xpAmount.ToString();
        }
    }
}
