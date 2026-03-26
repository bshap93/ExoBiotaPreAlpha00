using System;
using System.Collections.Generic;
using Helpers.Events.Terminals;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.SceneManagers
{
    public class ElevatorManager : MonoBehaviour, ICoreGameService, MMEventListener<ElevatorRootSystemEvent>
    {
        [Tooltip("Default elevator positions loaded when no save file exists yet.")]
        public ElevatorData[] startingElevatorData;

        [SerializeField] bool autoSave;

        readonly Dictionary<string, string> _elevatorDestinations = new();

        bool _dirty;
        string _savePath;

        public static ElevatorManager Instance { get; private set; }

        // ── Unity lifecycle ────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Reset();
                return;
            }

            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        // ── ICoreGameService ───────────────────────────────────────────────────

        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("ElevatorDestinations", _elevatorDestinations, path);
            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();
            _elevatorDestinations.Clear();

            if (ES3.KeyExists("ElevatorDestinations", path))
            {
                var loaded = ES3.Load<Dictionary<string, string>>("ElevatorDestinations", path);
                foreach (var kvp in loaded)
                    _elevatorDestinations[kvp.Key] = kvp.Value;
            }
            else
            {
                foreach (var data in startingElevatorData)
                    _elevatorDestinations[data.ElevatorSystemUniqueID] = data.DestinationID;
            }

            _dirty = false;
        }

        public void Reset()
        {
            _elevatorDestinations.Clear();

            // Seed from designer-defined defaults
            if (startingElevatorData != null)
                foreach (var data in startingElevatorData)
                    _elevatorDestinations[data.ElevatorSystemUniqueID] = data.DestinationID;

            _dirty = true;
            ConditionalSave();
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
                Save();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ElevatorSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        // ── Event listener ─────────────────────────────────────────────────────

        /// <summary>
        ///     The elevator event is handled here so the manager can persist the new
        ///     destination as soon as a move is commanded — the ElevatorRootSystem
        ///     also calls <see cref="SetDestination" /> on arrival for a confirmed write.
        /// </summary>
        public void OnMMEvent(ElevatorRootSystemEvent eventType)
        {
            // Pre-emptively record the commanded destination so that if the game
            // is force-quit mid-travel the elevator restores close to where it was
            // headed rather than its last confirmed stop.
            if (!string.IsNullOrEmpty(eventType.ElevatorSystemId) &&
                !string.IsNullOrEmpty(eventType.DestinationId))
                SetDestination(eventType.ElevatorSystemId, eventType.DestinationId);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Records and optionally auto-saves a new destination for an elevator.</summary>
        public void SetDestination(string elevatorID, string destinationID)
        {
            if (string.IsNullOrEmpty(elevatorID)) return;
            _elevatorDestinations[elevatorID] = destinationID;
            MarkDirty();
            ConditionalSave();
        }

        /// <summary>Returns the last known destination ID for an elevator, or null.</summary>
        public string GetDestination(string elevatorSystemUniqueID)
        {
            return _elevatorDestinations.GetValueOrDefault(elevatorSystemUniqueID);
        }

        // ── Nested types ───────────────────────────────────────────────────────

        [Serializable]
        public class ElevatorData
        {
            public string ElevatorSystemUniqueID;
            public string DestinationID;
        }
    }
}
