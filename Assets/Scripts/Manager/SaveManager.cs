using System;
using System.Collections;
using System.Collections.Generic;
using FirstPersonPlayer.FPNPCs;
using Helpers.Events;
using Helpers.ScriptableObjects;
using Inventory;
using Manager.DialogueScene;
using Manager.FirstPerson;
using Manager.Global;
using Manager.PlayerDataManagers;
using Manager.ProgressionMangers;
using Manager.SceneManagers;
using Manager.SceneManagers.Pickable;
using Manager.Settings;
using Manager.StateManager;
using Manager.Status;
using Manager.UI;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager
{
    public struct ResetStateChangedEvent
    {
        public bool IsReset;

        public static void Trigger(bool v)
        {
            MMEventManager.TriggerEvent(new ResetStateChangedEvent { IsReset = v });
        }
    }


    [Serializable]
    public enum GlobalManagerType
    {
        PlayerSave,
        PickablesSave,
        GlobalInventorySave,
        SpawnSave,
        ScenePersistenceSave,
        DialogueSave,
        ObjectivesSave,
        DoorSave,
        DestructablesSave,
        ItemDiscoverySave,
        ExaminationSave,
        BiologicalSamplingSave,
        PlayerDeath,
        GamePOISave,
        BioSamplesSave,
        BioOrganismSave,
        PickableLocationSave,
        TutorialSave,
        ContaminationSave,
        PlayerStatusEffectSave,
        StatefulPickables,
        GlobalSettingsSave,
        CurrencySave,
        InGameTimeSave,
        MachineStateSave,
        InfectionManagerSave,
        TriggerColliderSave,
        AttributesSave,
        CreatureStateSave,
        HotbarSave,
        // BioticAbilitiesSave,
        ToolsStateSave,
        LevelingSave,
        TerminalsSave,
        FriendlyNPCSave,
        CheckpointsSave,
        ElevatorSave,
        PlaytestSettingsSave,
        JournalEntrySave,
        BarrierStateSave,
        EntryProviderStateSave
    }

    public enum LocalManagerType
    {
        PickableManager,
        LocalInventoriesManager,
        DestructableManager
    }

    public class SaveManager : MonoBehaviour, MMEventListener<SaveDataEvent>, MMEventListener<ResetDataEvent>
    {
        public const string SaveFileName = "GameSave.es3";
        public const string SceneSaveFileName = "SceneSave.es3";
        public const string MessageSaveFileName = "MessageSave.es3";
        [SerializeField] MMFeedbacks saveFeedbacks;


        public CharacterStatProfile initialCharacterStatProfile;

        public bool autoSave = true;

        // Current save slot index
        [SerializeField] int saveSlot;

        public bool saveManagersDontDestroyOnLoad = true;

        [FormerlySerializedAs("_config")] [SerializeField]
        SaveManagerConfig saveConfig = new();

        public static SaveManager Instance { get; private set; }

        public bool IsInResetState { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            LoadAll();
        }

        void OnEnable()
        {
            this.MMEventStartListening<SaveDataEvent>();
            this.MMEventStartListening<ResetDataEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<SaveDataEvent>();
            this.MMEventStopListening<ResetDataEvent>();
        }

        // on application quit, save all data
        public void OnApplicationQuit()
        {
            if (autoSave)
            {
                Debug.Log("SaveManager: Application quitting, saving all data.");
                StartCoroutine(SaveAllThenWait());
            }
        }


        public void OnMMEvent(ResetDataEvent eventType)
        {
            ResetGameSave();
        }

        public void OnMMEvent(SaveDataEvent eventType)
        {
            SaveAll();
            // Debug.Log("SaveManager: SaveDataEvent received, saving all data.");
        }

        public void ApplyConfig(SaveManagerConfig config)
        {
            saveConfig = config ?? new SaveManagerConfig();
        }

        public void SetResetState(bool value)
        {
            if (IsInResetState == value) return;
            IsInResetState = value;
            ResetStateChangedEvent.Trigger(value);
        }

        public void LoadAll()
        {
            PlayerCurrencyManager.Instance.Load();
            PlayerMutableStatsManager.Instance.Load();
            PlayerSpawnManager.Instance.Load();
            ScenePersistenceManager.Instance.Load();
            GlobalInventoryManager.Instance.Load();
            DialogueManager.Instance.Load();
            ObjectivesManager.Instance.Load();
            if (DoorManager.Instance != null)
                DoorManager.Instance.Load();

            PickableManager.Instance.Load();
            DestructableManager.Instance.Load();
            if (ExaminationManager.Instance != null)
                ExaminationManager.Instance.Load();

            PlayerDeathManager.Instance.Load();
            CoreGamePOIManager.Instance.Load();
            BioSamplesManager.Instance.Load();
            BioOrganismManager.Instance.Load();
            PickableLocationManager.Instance.Load();
            PlayerStatusEffectManager.Instance.Load();
            StatefulPickableManager.Instance.Load();
            GlobalSettingsManager.Instance.Load();
            InGameTimeManager.Instance.Load();
            MachineStateManager.Instance.Load();
            TriggerColliderManager.Instance.Load();
            AttributesManager.Instance.Load();
            CreatureStateManager.Instance.Load();
            ConditionalBarrierManager.Instance.Load();
            JournalEntryProviderManager.Instance.Load();

            ToolsStateManager.Instance.Load();
            LevelingManager.Instance.Load();
            FriendlyNPCManager.Instance.Load();

            if (PlaytestSettingsManager.Instance != null)
                PlaytestSettingsManager.Instance.Load();

            TerminalManager.Instance.Load();
            CheckpointManager.Instance.Load();

            ElevatorManager.Instance.Load();
            JournalEntryManager.Instance.Load();

            if (!saveConfig.DisabledGlobalManagers.Contains(GlobalManagerType.TutorialSave))
                TutorialManager.Instance?.Load();

            // if (!saveConfig.DisabledGlobalManagers.Contains(GlobalManagerType.InfectionManagerSave))
            //     InfectionManager.Instance.Load();

            HotbarManager.Instance.Load();

            LoadedManagerEvent.Trigger(ManagerType.All);
        }


        /// <summary>
        ///     Synchronously saves all global game data.
        /// </summary>
        public void SaveAll()
        {
            saveFeedbacks?.PlayFeedbacks();
            // Save global managers
            PlayerCurrencyManager.Instance.Save();
            // Save Game State - Not yet implemented
            // GameStateManager.Instance.SaveGameState();
            PlayerMutableStatsManager.Instance.Save();
            PlayerSpawnManager.Instance.Save();
            ScenePersistenceManager.Instance.SaveCurrentScene();
            GlobalInventoryManager.Instance.Save();
            DialogueManager.Instance.Save();
            ObjectivesManager.Instance.Save();
            if (DoorManager.Instance != null)
                DoorManager.Instance.Save();

            PickableManager.Instance.Save();
            DestructableManager.Instance.Save();
            if (ExaminationManager.Instance != null)
                ExaminationManager.Instance.Save();

            PlayerDeathManager.Instance.Save();
            CoreGamePOIManager.Instance.Save();
            BioSamplesManager.Instance.Save();
            BioOrganismManager.Instance.Save();
            PickableLocationManager.Instance.Save();
            PlayerStatusEffectManager.Instance.Save();
            StatefulPickableManager.Instance.Save();
            // GlobalSettingsManager.Instance.Save();
            InGameTimeManager.Instance.Save();
            MachineStateManager.Instance.Save();
            TriggerColliderManager.Instance.Save();
            AttributesManager.Instance.Save();
            // State Managers 
            CreatureStateManager.Instance.Save();
            ConditionalBarrierManager.Instance.Save();
            JournalEntryProviderManager.Instance.Save();


            ToolsStateManager.Instance.Save();
            LevelingManager.Instance.Save();
            TerminalManager.Instance.Save();
            FriendlyNPCManager.Instance.Save();

            if (PlaytestSettingsManager.Instance != null)
                PlaytestSettingsManager.Instance.Save();

            ElevatorManager.Instance.Save();

            CheckpointManager.Instance.Save();

            JournalEntryManager.Instance.Save();

            if (!saveConfig.DisabledGlobalManagers.Contains(GlobalManagerType.TutorialSave))
                TutorialManager.Instance?.Save();

            // if (!saveConfig.DisabledGlobalManagers.Contains(GlobalManagerType.InfectionManagerSave))
            //     InfectionManager.Instance.Save();

            HotbarManager.Instance.Save();

            SetResetState(false);
        }

        public void ResetGameSave()
        {
            // Reset global managers
            PlayerCurrencyManager.Instance.ResetData();
            PlayerMutableStatsManager.Instance.Reset();
            PlayerSpawnManager.Instance.Reset();
            ScenePersistenceManager.Instance.Reset();
            GlobalInventoryManager.Instance.Reset();
            DialogueManager.Instance.Reset();
            ObjectivesManager.Instance.Reset();
            if (DoorManager.Instance != null)
                DoorManager.Instance.Reset();

            PickableManager.Instance.Reset();
            DestructableManager.Instance.Reset();
            if (ExaminationManager.Instance != null)
                ExaminationManager.Instance.Reset();

            PlayerDeathManager.Instance.Reset();
            CoreGamePOIManager.Instance.Reset();
            BioSamplesManager.Instance.Reset();
            BioOrganismManager.Instance.Reset();
            PickableLocationManager.Instance.Reset();
            PlayerStatusEffectManager.Instance.Reset();
            StatefulPickableManager.Instance.Reset();

            InGameTimeManager.Instance.Reset();
            MachineStateManager.Instance.Reset();
            TriggerColliderManager.Instance.Reset();
            AttributesManager.Instance.Reset();
            CreatureStateManager.Instance.Reset();
            ConditionalBarrierManager.Instance.Reset();
            JournalEntryManager.Instance.Reset();

            ToolsStateManager.Instance.Reset();
            LevelingManager.Instance.Reset();
            TerminalManager.Instance.Reset();
            FriendlyNPCManager.Instance.Reset();
            CheckpointManager.Instance.Reset();

            ElevatorManager.Instance.Reset();

            JournalEntryManager.Instance.Reset();

            if (PlaytestSettingsManager.Instance != null)
                PlaytestSettingsManager.Instance.Reset();


            if (!saveConfig.DisabledGlobalManagers.Contains(GlobalManagerType.TutorialSave))
                TutorialManager.Instance?.Reset();

            // if (!saveConfig.DisabledGlobalManagers.Contains(GlobalManagerType.InfectionManagerSave))
            //     InfectionManager.Instance.Reset();

            HotbarManager.Instance.Reset();

            SetResetState(true);
        }

        public void ResetSettings()
        {
            GlobalSettingsManager.Instance.Reset();
            // Add additional settings classes here
        }

        IEnumerator SaveAllThenWait()
        {
            SaveAll();
            AlertEvent.Trigger(AlertReason.SavingGame, "Saving game...");
            yield return new WaitForSeconds(1);
        }


        public string GetGlobalSaveFilePath(GlobalManagerType globalManagerType)
        {
            switch (globalManagerType)
            {
                case GlobalManagerType.PlayerSave:
                    return "PlayerSave.es3";
                case GlobalManagerType.PickablesSave:
                    return "PickablesSave.es3";
                case GlobalManagerType.GlobalInventorySave:
                    return "GlobalInventorySave.es3";
                case GlobalManagerType.ScenePersistenceSave:
                    return "ScenePersistenceSave.es3";
                case GlobalManagerType.SpawnSave:
                    return "SpawnSave.es3";
                case GlobalManagerType.DialogueSave:
                    return "DialogueSave.es3";
                case GlobalManagerType.ObjectivesSave:
                    return "ObjectivesSave.es3";
                case GlobalManagerType.DoorSave:
                    return "DoorSave.es3";
                case GlobalManagerType.DestructablesSave:
                    return "DestructablesSave.es3";
                case GlobalManagerType.ItemDiscoverySave:
                    return "ItemDiscoverySave.es3";
                case GlobalManagerType.ExaminationSave:
                    return "ExaminationSave.es3";
                case GlobalManagerType.BiologicalSamplingSave:
                    return "BiologicalSamplingSave.es3";
                case GlobalManagerType.PlayerDeath:
                    return "PlayerDeath.es3";
                case GlobalManagerType.GamePOISave:
                    return "GamePOISave.es3";
                case GlobalManagerType.BioSamplesSave:
                    return "BioSamplesSave.es3";
                case GlobalManagerType.BioOrganismSave:
                    return "BioOrganismSave.es3";
                case GlobalManagerType.PickableLocationSave:
                    return "PickableLocationSave.es3";
                case GlobalManagerType.TutorialSave:
                    return "TutorialSave.es3";
                case GlobalManagerType.ContaminationSave:
                    return "ContaminationSave.es3";
                case GlobalManagerType.PlayerStatusEffectSave:
                    return "PlayerStatusEffectSave.es3";
                case GlobalManagerType.StatefulPickables:
                    return "StatefulPickables.es3";
                case GlobalManagerType.GlobalSettingsSave:
                    return "GlobalSettingsSave.es3";
                case GlobalManagerType.CurrencySave:
                    return "CurrencySave.es3";
                case GlobalManagerType.InGameTimeSave:
                    return "InGameTimeSave.es3";
                case GlobalManagerType.MachineStateSave:
                    return "MachineStateSave.es3";
                case GlobalManagerType.InfectionManagerSave:
                    return "InfectionManagerSave.es3";
                case GlobalManagerType.TriggerColliderSave:
                    return "TriggerColliderSave.es3";
                case GlobalManagerType.AttributesSave:
                    return "AttributesSave.es3";
                case GlobalManagerType.CreatureStateSave:
                    return "CreatureStateSave.es3";
                case GlobalManagerType.HotbarSave:
                    return "HotbarSave.es3";
                // case GlobalManagerType.BioticAbilitiesSave:
                //     return "BioticAbilitiesSave.es3";
                case GlobalManagerType.ToolsStateSave:
                    return "ToolsStateSave.es3";
                case GlobalManagerType.LevelingSave:
                    return "LevelingSave.es3";
                case GlobalManagerType.TerminalsSave:
                    return "TerminalsSave.es3";
                case GlobalManagerType.FriendlyNPCSave:
                    return "FriendlyNPCSave.es3";
                case GlobalManagerType.CheckpointsSave:
                    return "CheckpointsSave.es3";
                case GlobalManagerType.ElevatorSave:
                    return "ElevatorSave.es3";
                case GlobalManagerType.PlaytestSettingsSave:
                    return "PlaytestSettingsSave.es3";
                case GlobalManagerType.JournalEntrySave:
                    return "JournalEntrySave.es3";
                case GlobalManagerType.BarrierStateSave:
                    return "BarrierStateSave.es3";
                case GlobalManagerType.EntryProviderStateSave:
                    return "EntryProviderStateSave.es3";
                default:

                    Debug.LogError($"Unknown manager type: {globalManagerType}");
                    return string.Empty;
            }
        }

        public string GetLocalSaveFilePath(LocalManagerType localManagerType, string sceneName)
        {
            switch (localManagerType)
            {
                case LocalManagerType.PickableManager:
                    return $"{sceneName}_PickablesSave.es3";
                case LocalManagerType.LocalInventoriesManager:
                    return $"{sceneName}_LocalInventoriesSave.es3";
                case LocalManagerType.DestructableManager:
                    return $"{sceneName}_DestructableSave.es3";
                default:
                    Debug.LogError($"Unknown local manager type: {localManagerType}");
                    return string.Empty;
            }
        }

        public IEnumerable<string> GetAllSceneSavePaths(string sceneName)
        {
            // 1) the one catch-all file (if you keep using it)
            yield return GetSceneFilePath(sceneName);

            // 2) one file per LocalManagerType (Pickable, Destructable…)
            foreach (LocalManagerType t in Enum.GetValues(typeof(LocalManagerType)))
                yield return GetLocalSaveFilePath(t, sceneName);
        }

        public string GetSceneFilePath(string sceneName)
        {
            return $"{sceneName}_SceneSave.es3";
        }

        [Serializable]
        public class SaveSlot
        {
            [FormerlySerializedAs("SaveSlotId")] public string saveSlotId;
            [FormerlySerializedAs("SaveSlotName")] public string saveSlotName;
        }
    }
}
