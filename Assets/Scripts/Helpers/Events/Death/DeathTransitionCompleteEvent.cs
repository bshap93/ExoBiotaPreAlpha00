using MoreMountains.Tools;

namespace Helpers.Events.Death
{
    public struct DeathTransitionCompleteEvent
    {
        static DeathTransitionCompleteEvent _e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(_e);
        }
    }
}
