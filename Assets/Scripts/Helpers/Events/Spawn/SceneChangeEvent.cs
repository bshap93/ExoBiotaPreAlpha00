using MoreMountains.Tools;

namespace Helpers.Events.Spawn
{
    public enum SceneChangeType
    {
        Elevator
    }

    public struct SceneChangeEvent
    {
        static SceneChangeEvent _e;
        public SceneChangeType SceneChangeType;

        public string SceneName;
        public string TravelChannelId;
        public string SpawnPointId;
        public string BridgeName;

        public static void Trigger(SceneChangeType sceneChangeType, string travelChannelId, string sceneName, string spawnPointId)
        {
            _e.SceneChangeType = sceneChangeType;
            _e.TravelChannelId = travelChannelId;
            _e.SceneName = sceneName;
            _e.SpawnPointId = spawnPointId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
