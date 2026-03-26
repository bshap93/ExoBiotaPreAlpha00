using Structs;
using UnityEngine;

namespace LevelConstruct
{
    /// <summary>
    ///     Static holder for passing spawn info to the Bridge scene
    /// </summary>
    public static class BridgeData
    {
        public static SpawnInfo TargetSpawn { get; private set; }
        public static bool HasPendingSpawn => TargetSpawn != null;

        /// <summary>
        ///     Set the target spawn before loading the Bridge scene
        /// </summary>
        public static void SetTarget(string sceneName, GameMode mode, string spawnPointId)
        {
            TargetSpawn = new SpawnInfo
            {
                SceneName = sceneName,
                Mode = mode,
                SpawnPointId = spawnPointId
            };

            Debug.Log($"[BridgeData] Target set: {sceneName} @ {spawnPointId} ({mode})");
        }

        /// <summary>
        ///     Set the target spawn before loading the Bridge scene
        /// </summary>
        public static void SetTarget(SpawnInfo spawnInfo)
        {
            TargetSpawn = spawnInfo;
            Debug.Log($"[BridgeData] Target set: {spawnInfo.SceneName} @ {spawnInfo.SpawnPointId} ({spawnInfo.Mode})");
        }

        /// <summary>
        ///     Consume and clear the target spawn (call this from BootLoader)
        /// </summary>
        public static SpawnInfo ConsumeTarget()
        {
            var target = TargetSpawn;
            TargetSpawn = null;
            return target;
        }

        /// <summary>
        ///     Clear without consuming
        /// </summary>
        public static void Clear()
        {
            TargetSpawn = null;
        }
    }
}
