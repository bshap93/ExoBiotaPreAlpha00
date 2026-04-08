using System;
using System.Collections.Generic;
using Manager;
using OWPData.Structs;
using Sirenix.OdinInspector;
using Structs;

namespace EditorScripts

{
    [Serializable]
    public struct SpawnInfoEditor
    {
        [ValueDropdown(nameof(GetSceneNames))] public string SceneName;

        [ValueDropdown(nameof(GetSpawnPoints))]
        public string SpawnPointId;

        public GameMode Mode;

        [ValueDropdown(nameof(GetOverSceneNames))]
        public string OverSceneName;

        public SpawnInfo ToSpawnInfo()
        {
            return new SpawnInfo
            {
                SceneName = SceneName,
                SpawnPointId = SpawnPointId,
                Mode = Mode,
                OverSceneName = OverSceneName
            };
        }

        static IEnumerable<string> GetSceneNames()
        {
            return PlayerSpawnManager.GetSceneOptions();
        }

        static IEnumerable<string> GetOverSceneNames()
        {
            return PlayerSpawnManager.GetOverSceneOptions();
        }

        IEnumerable<string> GetSpawnPoints()
        {
            return PlayerSpawnManager.GetSpawnPointIdOptions();
        }
    }
}
