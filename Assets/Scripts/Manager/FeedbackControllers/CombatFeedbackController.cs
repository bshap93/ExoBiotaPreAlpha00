using Helpers.Events.Combat;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.FeedbackControllers
{
    public class CombatFeedbackController : MonoBehaviour, MMEventListener<CombatFeedbacksEvent>
    {
        [SerializeField] MMFeedbacks staminaRestoredFeedbacks;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(CombatFeedbacksEvent eventType)
        {
            if (eventType.CombatFeedbackType == CombatFeedbackType.StaminaIsFullAgain)
                staminaRestoredFeedbacks?.PlayFeedbacks();
        }
    }
}
