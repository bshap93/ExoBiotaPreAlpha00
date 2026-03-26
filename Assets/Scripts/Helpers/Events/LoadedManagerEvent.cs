using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum ManagerType
    {
        All
    }

    public struct LoadedManagerEvent
    {
        private static LoadedManagerEvent _e;

        public ManagerType ManagerType;

        public static void Trigger(ManagerType managerType)
        {
            _e.ManagerType = managerType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}