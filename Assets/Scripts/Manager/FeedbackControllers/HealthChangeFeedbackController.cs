using Helpers.Events.Status;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.FeedbackControllers
{
    public class HealthChangeFeedbackController : MonoBehaviour, MMEventListener<PlayerStatsEvent>
    {
        [Header("Feedbacks")] [SerializeField] MMFeedbacks defaultDamageFeedback;
        [SerializeField] MMFeedbacks fallSpecificFeedback;
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
            if (eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentHealth &&
                eventType.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                switch (eventType.Cause)
                {
                    case PlayerStatsEvent.StatChangeCause.FallDamage:
                        fallSpecificFeedback?.PlayFeedbacks(eventType.SourcePosition);
                        break;

                    case PlayerStatsEvent.StatChangeCause.DecontaminationChamber:
                        defaultDamageFeedback?.PlayFeedbacks(eventType.SourcePosition);
                        break;
                    default:
                        return;
                }


            // fallSpecificFeedback?.PlayFeedbacks();
        }
    }
}
