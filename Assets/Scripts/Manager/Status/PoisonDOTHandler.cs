using System.Collections;
using System.Collections.Generic;
using Helpers.Events.Combat;
using Helpers.Events.Status;
using Manager.Status.Scriptable;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.Status
{
    /// <summary>
    ///     Attach to the Player. Listens for a poison status effect being applied or removed.
    ///     While active, drains health at a configurable rate for a configurable duration.
    ///     Automatically removes the status effect when the duration expires.
    ///     An antidote (or anything that removes the effect via PlayerStatusEffectEvent) stops the drain immediately.
    /// </summary>
    public class PoisonDOTHandler : MonoBehaviour, MMEventListener<PlayerStatusEffectEvent>
    {
        // [Header("Poison Settings")] [Tooltip("Must match the effectID used by CreaturePoisonAOE")] [SerializeField]
        // string poisonEffectID = "Poison";

        [SerializeField] PlayerStatusEffectManager statusEffectManager;

        [SerializeField] List<string> poisonEffectIDs;

        [SerializeField] MMFeedbacks antidoteAppliedFeedbacks;

        bool _isPoisoned;

        // [Tooltip("Health lost per second while poisoned")] [SerializeField]
        // float damagePerSecond = 3f;
        //
        // [Tooltip("How long the poison lasts before auto-clearing (seconds)")] [SerializeField]
        // float poisonDuration = 10f;
        // StatusEffect _poisonEffect;

        Coroutine _poisonRoutine;

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(PlayerStatusEffectEvent eventType)
        {
            if (eventType.EffectID == "Poison" &&
                eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.RemoveAllOfAKind)
            {
                antidoteAppliedFeedbacks.PlayFeedbacks();
                StopPoison();
                return;
            }

            var eventStatusEffect = statusEffectManager.GetStatusEffectByID(eventType.EffectID);
            // Only react to outbound events (confirmed applied/removed by the manager)
            if (eventType.Direction != PlayerStatusEffectEvent.DirectionOfEvent.Outbound) return;
            // if (eventType.EffectID != poisonEffectID) return;

            if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.Apply &&
                poisonEffectIDs.Contains(eventType.EffectID))
                StartPoison(eventStatusEffect);
            else if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.Remove &&
                     poisonEffectIDs.Contains(eventType.EffectID))
                StopPoison();
        }

        void StartPoison(StatusEffect statusEffect)
        {
            if (!statusEffect.causesPoisoning) return;
            if (_isPoisoned) return;
            _isPoisoned = true;
            StatusDebuffEvent.Trigger(
                StatusDebuffEvent.StatusDebuffEventType.Apply,
                StatusDebuffEvent.DebuffType.Poison, statusEffect.effectID);

            _poisonRoutine = StartCoroutine(PoisonDrainRoutine(statusEffect));
        }

        void StopPoison()
        {
            if (!_isPoisoned) return;
            _isPoisoned = false;

            if (_poisonRoutine != null)
            {
                StopCoroutine(_poisonRoutine);
                StatusDebuffEvent.Trigger(
                    StatusDebuffEvent.StatusDebuffEventType.Remove,
                    StatusDebuffEvent.DebuffType.Poison, string.Empty);

                _poisonRoutine = null;
            }
        }

        IEnumerator PoisonDrainRoutine(StatusEffect statusEffect)
        {
            var elapsed = 0f;

            while (elapsed < statusEffect.poisonDuration)
            {
                var damage = statusEffect.poisonDamagePerSecond * Time.deltaTime;

                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentHealth,
                    PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    damage
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Duration expired — remove the status effect (which also triggers StopPoison via the event)
            _poisonRoutine = null;
            _isPoisoned = false;

            StatusDebuffEvent.Trigger(
                StatusDebuffEvent.StatusDebuffEventType.Remove,
                StatusDebuffEvent.DebuffType.Poison, string.Empty);

            PlayerStatusEffectEvent.Trigger(
                PlayerStatusEffectEvent.StatusEffectEventType.Remove,
                statusEffect.effectID,
                statusEffect.catalogID,
                PlayerStatusEffectEvent.DirectionOfEvent.Inbound,
                StatusEffect.StatusEffectKind.None
            );
        }
    }
}
