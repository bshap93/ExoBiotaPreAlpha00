using System;
using MoreMountains.Tools;
using OWPData.Structs;
using Structs;

namespace Helpers.Events
{
    [Serializable]
    public enum CheckpointEventType
    {
        Visited
    }

    public struct CheckpointEvent
    {
        static CheckpointEvent _e;

        public CheckpointEventType CheckpointEventType;
        public string UniqueCheckpointId;

        public SpawnInfo SpawnInfo;


        public static void Trigger(CheckpointEventType checkpointEventType, string uniqueID, SpawnInfo spawnInfo)
        {
            _e.CheckpointEventType = checkpointEventType;
            _e.UniqueCheckpointId = uniqueID;
            _e.SpawnInfo = spawnInfo;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
