using Helpers.Events.Progression;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.FeedbackControllers
{
    public class ProgressionFeedbackController : MonoBehaviour, MMEventListener<XPEvent>, MMEventListener<LevelingEvent>, MMEventListener<PlayerSetsClassEvent>, MMEventListener<IncrementAttributeEvent>
    {
        [SerializeField] MMFeedbacks xpGainFeedbacks;
        [SerializeField] MMFeedbacks levelUpFeedbacks;
        [SerializeField] MMFeedbacks classChosenFeedbacks;
        [SerializeField] MMFeedbacks attributeIncrementedFeedbacks;


        void OnEnable()
        {
            this.MMEventStartListening<XPEvent>();
            this.MMEventStartListening<LevelingEvent>();
            this.MMEventStartListening<PlayerSetsClassEvent>();
            this.MMEventStartListening<IncrementAttributeEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<XPEvent>();
            this.MMEventStopListening<LevelingEvent>();
            this.MMEventStopListening<PlayerSetsClassEvent>();
            this.MMEventStopListening<IncrementAttributeEvent>();
        }
        public void OnMMEvent(LevelingEvent eventType)
        {
            if (eventType.EventType == LevelingEventType.LevelUp) levelUpFeedbacks?.PlayFeedbacks();
        }
        public void OnMMEvent(XPEvent eventType)
        {
            if (eventType.EventType == XPEventType.AwardXPToPlayer)
                if (!eventType.CausedLevelUp)
                    xpGainFeedbacks?.PlayFeedbacks();
        }
        public void OnMMEvent(PlayerSetsClassEvent eventType)
        {
            classChosenFeedbacks?.PlayFeedbacks();
            
        }
        public void OnMMEvent(IncrementAttributeEvent eventType)
        {
            attributeIncrementedFeedbacks?.PlayFeedbacks();
            
        }
    }
}
