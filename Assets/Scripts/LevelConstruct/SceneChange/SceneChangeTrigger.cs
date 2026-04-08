using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.UI;
using Manager;
using OWPData.Structs;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;

namespace LevelConstruct.SceneChange
{
    public class SceneChangeTrigger : MonoBehaviour
    {
        [ValueDropdown(nameof(GetSceneNames))] [SerializeField]
        string sceneToLoad;


        [ValueDropdown(nameof(GetSpawnPoints))] [SerializeField]
        string spawnPointId;

        // [FormerlySerializedAs("BridgeName")] public string bridgeName;


        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
            {
                SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);
                SaveDataEvent.Trigger();

                SpawnEvent.Trigger(
                    SpawnEventType.ToCaverns, sceneToLoad, GameMode.FirstPerson,
                    spawnPointId
                );
            }
        }

        static IEnumerable<string> GetSceneNames()
        {
            return PlayerSpawnManager.GetSceneOptions();
        }

        IEnumerable<string> GetSpawnPoints()
        {
            return PlayerSpawnManager.GetSpawnPointIdOptions();
        }
    }
}
