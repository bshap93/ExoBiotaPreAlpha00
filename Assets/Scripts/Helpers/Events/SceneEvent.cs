using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum SceneEventType
    {
        SceneLoaded,
        TogglePauseScene,
        PlayerPawnLoaded,
        PlayerRequestsQuit,
        PlayerRequestsMainMenu
    }

    public struct SceneEvent
    {
        static SceneEvent _e;

        public SceneEventType EventType;


        public static void Trigger(SceneEventType sceneEventType)
        {
            _e.EventType = sceneEventType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
