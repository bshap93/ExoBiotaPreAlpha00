using System;
using Helpers.Events;
using Manager;
using Manager.Global;
using Manager.ProgressionMangers;
using Manager.Settings;
using Manager.Status;
using Manager.Status.Scriptable;
using OWPData.Structs;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace LevelConstruct.Spawn
{
    public class SpawnCheckpoint : MonoBehaviour, IRequiresUniqueID
    {
        const float SpawnGraceDuration = 10f;

        static float _spawnedAtRealtime = float.MinValue;

        public string checkpointName;

        [SerializeField] string uniqueCheckpointId;
#if UNITY_EDITOR
        [ValueDropdown("GetListOfTags")] [SerializeField]
#endif
        string playerPawnTag = "FirstPersonPlayer";
        [FormerlySerializedAs("_point")] [SerializeField]
        SpawnPoint point;

        [SerializeField] bool useAsAutoSavePoint;

        CheckpointManager _checkpointManager;
        PlayerStatusEffectManager _playerStatusEffectManager;

        static bool IsInSpawnGracePeriod =>
            Time.realtimeSinceStartup - _spawnedAtRealtime < SpawnGraceDuration;


        void Awake()
        {
            if (point == null)
                point = GetComponent<SpawnPoint>();
        }

        void Start()
        {
            _checkpointManager = CheckpointManager.Instance;
            if (useAsAutoSavePoint)
                _playerStatusEffectManager = PlayerStatusEffectManager.Instance;
        }

        void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(playerPawnTag)) return;
            if (!other.CompareTag(playerPawnTag)) return;

            if (_checkpointManager == null || _checkpointManager.HasCheckpointBeenVisited(uniqueCheckpointId))
                return;


            if (IsInSpawnGracePeriod)
                // Debug.Log("[SpawnCheckpoint] Trigger suppressed — within spawn grace period.");
                return;

            var globalSettingsMgr = GlobalSettingsManager.Instance;
            if (globalSettingsMgr == null)
                // Debug.LogError("[SpawnCheckpoint] No GlobalSettingsManager found in scene.");
                return;

            if (!globalSettingsMgr.AutoSaveAtCheckpoints)
                // Debug.Log("[SpawnCheckpoint] Autosave at the checkpoint is disabled in Global Settings.");
                return;

            if (!useAsAutoSavePoint)
                // Debug.Log("[SpawnCheckpoint] This checkpoint is not set to be used as an autosave point.");
                return;

            if (_playerStatusEffectManager != null)
            {
                if (_playerStatusEffectManager.IsPlayerAffectedByStatusKind(StatusEffect.StatusEffectKind.Poison))
                {
                    Debug.Log(
                        "[SpawnCheckpoint] Player is currently poisoned. Checkpoint autosave skipped to avoid saving in a compromised state.");

                    return;
                }
            }
            else
            {
                Debug.LogWarning(
                    "[SpawnCheckpoint] PlayerStatusEffectManager not found. Unable to check for poison status before autosaving at checkpoint.");
            }

            var spawnInfo = new SpawnInfo
            {
                SceneName = gameObject.scene.name,
                Mode = GameStateManager.Instance.CurrentMode,
                SpawnPointId = point.Id
            };

            PlayerSpawnManager.Instance.Save(spawnInfo);

            CheckpointEvent.Trigger(
                CheckpointEventType.Visited,
                uniqueCheckpointId, spawnInfo);

            SaveDataEvent.Trigger();


            AlertEvent.Trigger(
                AlertReason.AutoSave, "Saved at checkpoint: " + checkpointName, "Checkpoint Reached", AlertType.Basic, 2f);
            // CheckpointEvent.Trigger(spawnInfo);
        }
        public string UniqueID => uniqueCheckpointId;
        public void SetUniqueID()
        {
            uniqueCheckpointId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueCheckpointId);
        }

        public static void NotifySpawned()
        {
            _spawnedAtRealtime = Time.realtimeSinceStartup;
        }

#if UNITY_EDITOR
        public static string[] GetListOfTags()
        {
            return InternalEditorUtility.tags;
        }
#endif
    }
}
