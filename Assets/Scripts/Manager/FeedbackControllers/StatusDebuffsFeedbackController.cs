using Helpers.Events.Status;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.FeedbackControllers
{
    public class StatusDebuffsFeedbackController : MonoBehaviour, MMEventListener<StatusDebuffEvent>
    {
        [SerializeField] MMFeedbacks poisonApplyFeedbacks;
        [SerializeField] MMFeedbacks poisonRemoveFeedbacks;
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(StatusDebuffEvent eventType)
        {
            if (eventType.Type == StatusDebuffEvent.StatusDebuffEventType.Apply)
                switch (eventType.Debuff)
                {
                    case StatusDebuffEvent.DebuffType.Poison:
                        poisonApplyFeedbacks.PlayFeedbacks();
                        break;
                    default:
                        Debug.LogWarning("Unhandled DebuffType: " + eventType.Debuff);
                        break;
                }
            else if (eventType.Type == StatusDebuffEvent.StatusDebuffEventType.Remove)
                switch (eventType.Debuff)
                {
                    case StatusDebuffEvent.DebuffType.Poison:
                        poisonRemoveFeedbacks.PlayFeedbacks();
                        break;
                    default:
                        Debug.LogWarning("Unhandled DebuffType: " + eventType.Debuff);
                        break;
                }
            else if (eventType.Type == StatusDebuffEvent.StatusDebuffEventType.Resisted)
                switch (eventType.Debuff)
                {
                    case StatusDebuffEvent.DebuffType.Poison:
                        poisonRemoveFeedbacks.PlayFeedbacks();
                        break;
                }
            else Debug.LogWarning("Unhandled StatusDebuffEventType: " + eventType.Type);
        }
    }
}
