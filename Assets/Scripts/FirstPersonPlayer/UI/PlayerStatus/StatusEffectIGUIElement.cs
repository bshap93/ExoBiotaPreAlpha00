using System.Collections.Generic;
using Manager.Status;
using Manager.Status.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstPersonPlayer.UI.PlayerStatus
{
    public class StatusEffectIGUIElement : MonoBehaviour
    {
        public Image statusEffectIcon;
        public TMP_Text statusEffectNameText;

        public GameObject statChangeElementPrefab;

        public Transform gridOfEffects;

        public List<StatsBuffMicroElement> statsBuffElements = new();

        public void Populate(StatusEffect statusEffect)
        {
            Cleanup();
            if (statusEffect == null) return;

            statusEffectIcon.sprite = statusEffect.effectIcon;
            statusEffectNameText.text = statusEffect.effectName;
            foreach (var statChange in statusEffect.statsChanges)
            {
                // Instantiate a new stat change element
                var newElement = Instantiate(statChangeElementPrefab, gridOfEffects);
                var microElement = newElement.GetComponent<StatsBuffMicroElement>();
                if (microElement != null)
                {
                    statsBuffElements.Add(microElement);
                    microElement.statBuffIcon.sprite = statChange.icon;
                    microElement.abbreviationOfStat.text =
                        PlayerStatusEffectManager.Instance.GetAbbreviation(statChange.statType);

                    var numStr = statChange.amount == 0f
                        ? $"{statChange.percent * 100}%"
                        : statChange.amount.ToString("F1");

                    microElement.statBuffNumText.text = statChange.isPositive ? $"+{numStr}" : $"-{numStr}";
                }
            }
        }

        void Cleanup()
        {
            foreach (Transform child in gridOfEffects) Destroy(child.gameObject);
            statsBuffElements.Clear();
        }
    }
}
