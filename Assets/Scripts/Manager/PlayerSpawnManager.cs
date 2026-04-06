using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.Spawn;
using Helpers.Interfaces;
using MoreMountains.Tools;
using Structs;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Static;

namespace Manager
{
    public class PlayerSpawnManager : MonoBehaviour, ICoreGameService, MMEventListener<SpawnEvent>,
        MMEventListener<CheckpointEvent>, MMEventListener<SpawnAssignmentEvent>
    {
        const string SpawnKey = "SpawnInfo";
        const string SpawnDictKey = "SpawnDict";

        [SerializeField] bool autoSave; // checkpoint-only by default

        [SerializeField] bool autoSaveAtCheckpoints = true;

        [FormerlySerializedAs("DefaultStartSpawn")] [SerializeField]
        string defaultStartSpawn = "StartSpawn";
        [SerializeField] string defaultStartScene = "AshpoolMine";

        bool _dirty;

        string _savePath;

        public SpawnInfo LastAssignedSpawn { get; private set; }

        public static SpawnInfo LastLoadedSpawn { get; private set; }

        public static PlayerSpawnManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _savePath = GetSaveFilePath();
            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("[PlayerSpawnManager] No save file found, forcing initial save...");
                ResetDefault();
                Save();
            }

            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening<CheckpointEvent>();
            this.MMEventStartListening<SpawnEvent>();
            this.MMEventStartListening<SpawnAssignmentEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<CheckpointEvent>();
            this.MMEventStopListening<SpawnEvent>();
            this.MMEventStopListening<SpawnAssignmentEvent>();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.SpawnSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty)
            {
                Save();
                _dirty = false;
            }
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void Save()
        {
            Save(LastLoadedSpawn);
            _dirty = false;
        }

        public void Load()
        {
            LastLoadedSpawn = LoadSlot();
        }

        public void Reset()
        {
            if (ES3.FileExists(_savePath))
                ES3.DeleteFile(_savePath); // <--- wipe old spawn save

            ResetDefault();
            _dirty = false;
            // ConditionalSave();
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }

        public void OnMMEvent(CheckpointEvent eventType)
        {
            if (autoSaveAtCheckpoints) Save(eventType.SpawnInfo);
        }
        public void OnMMEvent(SpawnAssignmentEvent eventType)
        {
            if (eventType.SpawnAssignmentEventType == SpawnAssignmentEventType.SetMostRecentSpawnPoint)
                LastAssignedSpawn = new SpawnInfo
                {
                    SceneName = eventType.SceneName,
                    SpawnPointId = eventType.SpawnPointID,
                    Mode = GameMode.FirstPerson
                };
        }

        public async void OnMMEvent(SpawnEvent eventType)
        {
            var req = new SpawnRequest(
                eventType.sceneName,
                eventType.gameMode, eventType.spawnPointId);


            await SpawnSystem.LoadAndSpawnAsync(req);
        }

        public bool HasSave()
        {
            if (_savePath == null) _savePath = GetSaveFilePath();
            return ES3.FileExists(_savePath) && ES3.KeyExists(SpawnKey, _savePath);
        }

        void ResetDefault()
        {
            var defaultSpawn = new SpawnInfo
            {
                SceneName = defaultStartScene,
                Mode = GameMode.FirstPerson,
                SpawnPointId = defaultStartSpawn
            };

            // Save(defaultSpawn);
            LastLoadedSpawn = defaultSpawn;
            _dirty = false;
        }


        public void Save(SpawnInfo spawn)
        {
            var dict = ES3.Load(SpawnDictKey, _savePath, new Dictionary<string, SpawnInfo>());
            dict[spawn.SpawnPointId] = spawn;
            ES3.Save(SpawnDictKey, dict, _savePath);
            ES3.Save(SpawnKey, spawn, _savePath);
            LastLoadedSpawn = spawn;
        }


        public SpawnInfo LoadSlot()
        {
            if (_savePath == null) _savePath = GetSaveFilePath();

            // 1. If either the file or the key is missing, create a default entry
            if (!ES3.FileExists(_savePath) || !ES3.KeyExists(SpawnKey, _savePath))
            {
                Debug.LogWarning($"[PlayerSpawnManager] Missing {SpawnKey} in {_savePath}. Writing default spawn.");
                ResetDefault();
            }

            // 2. Load safely, providing a fallback default in case of corruption
            var spawn = ES3.Load(
                SpawnKey, _savePath,
                new SpawnInfo // <- default value if the record is unreadable
                {
                    SceneName = defaultStartScene,
                    Mode = GameMode.DirigibleFlight,
                    SpawnPointId = defaultStartSpawn
                });

            if (string.IsNullOrEmpty(spawn.SceneName) || string.IsNullOrEmpty(spawn.SpawnPointId))
            {
                spawn.SceneName = defaultStartScene;
                spawn.SpawnPointId = defaultStartSpawn;
            }

            LastLoadedSpawn = spawn;
            return spawn;
        }

        public SpawnInfo LoadSlot(string spawnPointId) // <-- NEW OVERLOAD
        {
            var dict = ES3.Load(
                SpawnDictKey, _savePath,
                new Dictionary<string, SpawnInfo>());

            dict.TryGetValue(spawnPointId, out var info);
            return info; // null if not found
        }

        public static string[] GetSpawnPointIdOptions()
        {
            return new[]
            {
                // Overworld
                // "ScienceDockSpawn", "Mine01Dock", "MidFlightTestSpawn", "TestFPSpawn",
                // "EnterValleySpawn", "DockAshpoolMineSpawn",
                // Mine01
                // "Mine01DoorSpawn", "CorePatchSpawn", "BailoutFacilitySpawn", "UndergroundSpawnMine01",
                // "UndergroundSpawnMine02",
                // Bionics Lab
                "BionicsLabSpawn", "BionicsLab02Spawn",

                // Choked Caverns
                // Ashpool Mine,
                "AshpoolMineDoorSpawn", "c1_entrance", "m1_entrance", "apm_debug", "m1_exit",
                "C1_Bluehole_Intersection", "b1_foyer", "spawnpoint_elevator_upto_l1",
                "chapel_center", "chapel_exit", "hotspring_house", "WestEntrySpawnHSAdjCorr",
                "SouthEntrySpawnHSAdjCorr",
                "StartSpawn", "TerminalSpawn00", "FacilityUtilityC2Spawn", "DownwardPsgTopSpawn",
                "BattlefieldGateSpawn", "hotspring_gate", "BunkerSpawnL2",
                "spawnpoint_terr_gate", "mine_embankment_spawn",
                "StrengthAlleySpawn",
                // Kinship Magnium Mine,
                "ElevatorRoomInitSpawn", "treeside_spawn", "ShoalAlcoveSpawn",
                "SupervisorHouse", "OpeningToTeeGap", "SmallRoomL1Spawn",
                "SouthernTeeSpawn", "MineCavernEntrySpawn",
                "ScrapVillageMine", "MineEscapeTunnelEntrySpawn", "ElevatorRoomBunkerSpawn",
                // Lake Scene
                "FakeIslandSpawn"
            };
        }

        public static string[] GetSceneOptions()
        {
            return new[]
            {
                // "Mine01",
                "AshpoolMine",
                "FirstPersonTestbed",
                "KinshipMagniumMine",
                "FirstPersonTestbed02",
                "LakesideHollow"
            };
        }
        public static IEnumerable<string> GetOverSceneOptions()
        {
            return new[]
            {
                "TestOverScene", "MineOverScene"
            };
        }
    }
}
