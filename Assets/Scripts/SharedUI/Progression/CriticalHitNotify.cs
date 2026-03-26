using System;
using TMPro;
using UnityEngine;

namespace SharedUI.Progression
{
    public class CriticalHitNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text multiplierText;
        public void SetCriticalHitText(float multipler)
        {
            multiplierText.text = $"{Math.Round(multipler, 2)}";
        }
    }
}
