using MoreMountains.Tools;

namespace Helpers.Events.Triggering
{
    public struct MySceneTransitionAdditiveEvent
    {
        public enum MySceneTransEventType
        {
            Load,
            Unload,
            SetActiveScene
        }

        static MySceneTransitionAdditiveEvent _e;

        public string SceneName;
        public MySceneTransEventType EventType;

        public static void Trigger(MySceneTransEventType eventType, string sceneName)
        {
            _e.EventType = eventType;
            _e.SceneName = sceneName;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
