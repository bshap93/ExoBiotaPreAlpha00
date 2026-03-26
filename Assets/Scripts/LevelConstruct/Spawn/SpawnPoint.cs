using Manager;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;

namespace LevelConstruct.Spawn
{
    [RequireComponent(typeof(SpawnCheckpoint))]
    [RequireComponent(typeof(SphereCollider))]
    [DisallowMultipleComponent]
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] protected GameMode mode;

        [ValueDropdown("GetSpawnPointIdOptions")]
        public string spawnPointId;

        public string Id
        {
            get => spawnPointId;
            set => spawnPointId = value;
        }

        public GameMode Mode => mode;
        public Transform Xform => transform;

        static string[] GetSpawnPointIdOptions()
        {
            return PlayerSpawnManager.GetSpawnPointIdOptions();
        }
    }
}
