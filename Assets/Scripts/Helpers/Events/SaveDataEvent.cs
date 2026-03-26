using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct SaveDataEvent
    {
        private static SaveDataEvent _e;


        public static void Trigger()
        {
            MMEventManager.TriggerEvent(_e);
        }
    }
}