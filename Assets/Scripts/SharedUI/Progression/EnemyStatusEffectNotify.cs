using System.Globalization;
using Helpers.Events.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Progression
{
    public class EnemyStatusEffectNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text labelTxt;
        [SerializeField] Image statusEffectIcon;
        [SerializeField] TMP_Text valueTxt;
        public void SetStatusEffectText(EnemyStatusEffectType effectType, float value)
        {
            labelTxt.text = effectType.ToString();
            valueTxt.text = value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
