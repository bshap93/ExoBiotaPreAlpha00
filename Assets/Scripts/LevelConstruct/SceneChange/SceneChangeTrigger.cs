using System.Collections.Generic;
using EditorScripts;
using Helpers.Events;
using Helpers.Events.UI;
using Manager;
using OWPData.Structs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LevelConstruct.SceneChange
{
    public class SceneChangeTrigger : MonoBehaviour
    {
        // [ValueDropdown(nameof(GetSceneNames))] [SerializeField]
        // string sceneToLoad;
        //
        //
        // [ValueDropdown(nameof(GetSpawnPoints))] [SerializeField]
        // string spawnPointId;

        [SerializeField] [InlineProperty] [HideLabel]
        SpawnInfoEditor overrideSpawnInfo;

        // [FormerlySerializedAs("BridgeName")] public string bridgeName;


        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
            {
                SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);
                SaveDataEvent.Trigger();

                SpawnEvent.Trigger(
                    SpawnEventType.ToCaverns, overrideSpawnInfo.SceneName, GameMode.FirstPerson,
                    overrideSpawnInfo.SpawnPointId
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
