using MoreMountains.Tools;

namespace Helpers.Events.Spawn
{
    public enum SpawnAssignmentEventType
    {
        SetMostRecentSpawnPoint
    }

    public struct SpawnAssignmentEvent
    {
        static SpawnAssignmentEvent _e;

        public SpawnAssignmentEventType SpawnAssignmentEventType;
        public string SceneName;
        public string SpawnPointID;

        public static void Trigger(SpawnAssignmentEventType eventType, string sceneName, string spawnPointID)
        {
            _e.SpawnAssignmentEventType = eventType;
            _e.SceneName = sceneName;
            _e.SpawnPointID = spawnPointID;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
