using System;
using UnityEngine;

namespace Structs
{
    public enum GameMode
    {
        DirigibleFlight,
        FirstPerson,
        Overview,
        None,
        FreeLook
    }

    [Serializable]
    public class SpawnInfo
    {
        public string SceneName; // Name of the scene to spawn in
        public GameMode Mode;
        public string SpawnPointId;
        public Vector3 OverridePos;
        public Quaternion OverrideRot;
        public string OverSceneName;
    }
}
