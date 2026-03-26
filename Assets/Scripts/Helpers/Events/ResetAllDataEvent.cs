using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct ResetDataEvent
    {
        static ResetDataEvent _e;


        public static void Trigger()
        {
            MMEventManager.TriggerEvent(_e);
        }
    }
}
