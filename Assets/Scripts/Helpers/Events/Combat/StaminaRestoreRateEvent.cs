using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct StaminaRestoreRateEvent
    {
        static StaminaRestoreRateEvent _e;

        public float CurrentStaminaRestoreRate;

        public static void Trigger(float currentStaminaRestoreRate)
        {
            _e.CurrentStaminaRestoreRate = currentStaminaRestoreRate;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
