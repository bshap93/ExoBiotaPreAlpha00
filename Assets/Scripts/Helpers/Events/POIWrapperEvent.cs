using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct POIWrapperEvent
    {
        static POIWrapperEvent _e;

        public string UniqueId;
        public string SceneName;
        public POIWrapperEventType Type;

        public static void Trigger(string uniqueId, string sceneName, POIWrapperEventType type)
        {
            _e.UniqueId = uniqueId;
            _e.SceneName = sceneName;
            _e.Type = type;
            MMEventManager.TriggerEvent(_e);
        }
    }

    public enum POIWrapperEventType
    {
        StateChanged, // generic catch-all
        TrackedByObjective, // specifically being tracked
        Untracked,
        VisibilityChanged
    }
}
