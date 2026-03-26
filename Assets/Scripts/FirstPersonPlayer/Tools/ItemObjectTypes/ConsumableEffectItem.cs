using Helpers.Events.Combat;
using Helpers.Events.Status;
using Manager.Status.Scriptable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "New Consumable Effect Item", menuName = "Scriptable Objects/Items/Consumable Effect Item")]
    public class ConsumableEffectItem : MyBaseItem
    {
        [Header("Consumable Effect Item Settings")]
        public bool restoresHealth;
        [ShowIf("restoresHealth")] public float healthRestored;

        public bool restoresStamina;
        [ShowIf("restoresStamina")] public float staminaRestored;

        public bool curesPoisoning;

        public override bool Use(string playerID)
        {
            if (restoresHealth)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentHealth, PlayerStatsEvent.PlayerStatChangeType.Increase,
                    healthRestored);

            if (restoresStamina)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Increase,
                    staminaRestored);

            if (curesPoisoning)
            {
                PlayerStatusEffectEvent.Trigger(PlayerStatusEffectEvent.StatusEffectEventType.RemoveAllOfAKind, "Poison", null,
                    PlayerStatusEffectEvent.DirectionOfEvent.Inbound, StatusEffect.StatusEffectKind.Poison);
            }

            return true;
        }
    }
}
