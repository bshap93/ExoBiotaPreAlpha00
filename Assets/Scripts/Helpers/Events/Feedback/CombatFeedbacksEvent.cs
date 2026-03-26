using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public enum CombatFeedbackType
    {
        StaminaIsFullAgain
    }

    public struct CombatFeedbacksEvent
    {
        static CombatFeedbacksEvent _e;

        public CombatFeedbackType CombatFeedbackType;

        public static void Trigger(CombatFeedbackType feedbackType)
        {
            _e.CombatFeedbackType = feedbackType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
