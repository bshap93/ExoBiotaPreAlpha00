using MoreMountains.Tools;
using Structs;

namespace Helpers.Events
{
    public enum SpawnEventType
    {
        ToDock,
        ToMine,
        ToCaverns
    }

    public struct SpawnEvent
    {
        static SpawnEvent _e;

        public SpawnEventType spawnEventType; // unused for now, but can be used to differentiate events
        public string sceneName;
        public GameMode gameMode;
        public string spawnPointId;


        public static void Trigger(SpawnEventType spawnEventType, string sceneName, GameMode gameMode,
            string spawnPointId
        )
        {
            _e.sceneName = sceneName;
            _e.gameMode = gameMode;
            _e.spawnPointId = spawnPointId;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
