using System;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.FeedbackControllers
{
    public class PlayerStatsFeedbackController : MonoBehaviour, MMEventListener<PlayerStatsEvent>
    {
        [Header("Feedbacks")] [SerializeField] MMFeedbacks jabbarCrecheHealthDecreaseFeedbacks;
        [SerializeField] MMFeedbacks maxHealthDecreaseFeedbacks;
        [SerializeField] MMFeedbacks increasedCurrentMaxHealthFeedbacks;
        [SerializeField] MMFeedbacks increasedCurrentMaxStaminaFeedbacks;
        [SerializeField] MMFeedbacks increasedCurrentMaxContaminationFeedbacks;

        [Header("Low Health Warning")] [SerializeField]
        MMFeedbacks lowHealthFeedback;

        bool _lowHealth;

        void OnEnable()
        {
            this.MMEventStartListening();
        }


        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(PlayerStatsEvent eventType)
        {
            switch (eventType.StatType)
            {
                case PlayerStatsEvent.PlayerStat.CurrentHealth:
                    HandleHealthFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentStamina:
                    HandleStaminaFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentContamination:
                    HandleContaminationFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentMaxHealth:
                    HandleCurrentMaxHealthFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentStaminaRestoreRate:
                    HandleCurrentMaxStaminaFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentMaxContamination:
                    HandleCurrentMaxContaminationFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentMaxVision:
                    // Not implemented yet
                    break;
                case PlayerStatsEvent.PlayerStat.CurrentVision:
                    // Not implemented yet
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        void HandleCurrentMaxContaminationFeedback(PlayerStatsEvent eventType)
        {
        }
        void HandleCurrentMaxStaminaFeedback(PlayerStatsEvent eventType)
        {
        }
        void HandleCurrentMaxHealthFeedback(PlayerStatsEvent eventType)
        {
            if (eventType.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                AlertEvent.Trigger(
                    AlertReason.MaxHealthDecrease, "Your maximum health has decreased. Find regeneration tank soon.",
                    "Max Health Decrease");
        }

        void HandleHealthFeedback(PlayerStatsEvent eventType)
        {
            switch (eventType.ChangeType)
            {
                case PlayerStatsEvent.PlayerStatChangeType.Decrease:
                    HandleHealthDecreaseFeedback(eventType);
                    break;

                case PlayerStatsEvent.PlayerStatChangeType.Increase:
                    HandleHealthIncreaseFeedback(eventType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void HandleHealthDecreaseFeedback(PlayerStatsEvent eventType)
        {
            switch (eventType.Cause)
            {
                case PlayerStatsEvent.StatChangeCause.JabbarCreche:
                    jabbarCrecheHealthDecreaseFeedbacks.enabled = true;
                    jabbarCrecheHealthDecreaseFeedbacks?.PlayFeedbacks();
                    break;
                case PlayerStatsEvent.StatChangeCause.Other:
                case PlayerStatsEvent.StatChangeCause.FallDamage:

                    // other feedbacks here
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (PlayerMutableStatsManager.Instance.GetHealthFraction() < 0.4f) // threshold
            {
                if (_lowHealth) return; // Don't trigger multiple times if already in low health state
                _lowHealth = true;

                lowHealthFeedback?.PlayFeedbacks();
                AlertEvent.Trigger(
                    AlertReason.HealthWarning, "Your health is low. Find medical supplies soon.",
                    "Health Alert");
            }
        }


        void HandleHealthIncreaseFeedback(PlayerStatsEvent eventType)
        {
            if (eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentMaxHealth)
                increasedCurrentMaxHealthFeedbacks?.PlayFeedbacks();
            else if (eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentHealth)
                if (PlayerMutableStatsManager.Instance.GetHealthFraction() >= 0.4f)
                    _lowHealth = false;
        }

        void HandleStaminaFeedback(PlayerStatsEvent eventType)
        {
            switch (eventType.ChangeType)
            {
                case PlayerStatsEvent.PlayerStatChangeType.Decrease:
                    HandleStaminaDecreaseFeedback(eventType);
                    break;

                case PlayerStatsEvent.PlayerStatChangeType.Increase:
                    HandleStaminaIncreaseFeedback(eventType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void HandleStaminaDecreaseFeedback(PlayerStatsEvent eventType)
        {
        }

        void HandleStaminaRestoreFeedback(PlayerStatsEvent eventType)
        {
        }

        void HandleStaminaIncreaseFeedback(PlayerStatsEvent eventType)
        {
            if (eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentStaminaRestoreRate)
                increasedCurrentMaxStaminaFeedbacks?.PlayFeedbacks();
        }

        void HandleContaminationFeedback(PlayerStatsEvent eventType)
        {
            switch (eventType.ChangeType)
            {
                case PlayerStatsEvent.PlayerStatChangeType.Increase:
                    HandleContaminationIncreaseFeedback(eventType);
                    break;
                case PlayerStatsEvent.PlayerStatChangeType.Decrease
                    :
                    HandleContaminationDecreaseFeedback(eventType);
                    break;
            }
        }

        void HandleContaminationIncreaseFeedback(PlayerStatsEvent eventType)
        {
            if (eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentMaxContamination)
                increasedCurrentMaxContaminationFeedbacks?.PlayFeedbacks();
        }

        void HandleContaminationDecreaseFeedback(PlayerStatsEvent eventType)
        {
        }
    }
}
