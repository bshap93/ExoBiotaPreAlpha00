using System;
using System.Collections.Generic;
using Manager;
using OWPData.Structs;
using Sirenix.OdinInspector;

namespace EditorScripts

{
    [Serializable]
    public struct SpawnInfoEditor
    {
        [ValueDropdown(nameof(GetSceneNames))] [OnValueChanged(nameof(OnSceneChanged))]
        public string SceneName;

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
        // Clear the spawn point selection whenever the scene changes so the
        // inspector never holds a stale ID from the previous scene.
        void OnSceneChanged()
        {
            SpawnPointId = string.Empty;
        }

        static IEnumerable<string> GetOverSceneNames()
        {
            return PlayerSpawnManager.GetOverSceneOptions();
        }

        IEnumerable<string> GetSpawnPoints()
        {
            return PlayerSpawnManager.GetSpawnPointIdOptions(SceneName);
        }
    }
}
