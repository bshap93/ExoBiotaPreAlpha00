using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace OWPData.Structs
{
    [Serializable]
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
        [FormerlySerializedAs("SceneName")] public string sceneName; // Name of the scene to spawn in
        [FormerlySerializedAs("Mode")] public GameMode mode;
        [FormerlySerializedAs("SpawnPointId")] public string spawnPointId;
        [FormerlySerializedAs("OverridePos")] public Vector3 overridePos;
        [FormerlySerializedAs("OverrideRot")] public Quaternion overrideRot;
        [FormerlySerializedAs("OverSceneName")]
        public string overSceneName;
    }
}
