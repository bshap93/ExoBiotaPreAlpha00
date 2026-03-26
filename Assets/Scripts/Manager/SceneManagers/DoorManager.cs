using System;
using System.Collections.Generic;
using Helpers.Events.Machine;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.SceneManagers
{
    public class DoorManager : MonoBehaviour, ICoreGameService, MMEventListener<DoorEvent>
    {
        [Serializable]
        public enum DoorLockState
        {
            Locked,
            Unlocked,
            Inaccessible,
            None
        }

        [SerializeField] bool autoSave;

        bool _dirty;

        Dictionary<string, DoorLockState> _doorsLocked = new();


        string _savePath;
        public static DoorManager Instance { get; private set; }
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

        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("DoorsLockedState", _doorsLocked, path);

            _dirty = false;
        }

        public void Reset()
        {
            _doorsLocked.Clear();
            _dirty = true;
            ConditionalSave();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.DoorSave);
        }
        public void CommitCheckpointSave()
        {
            if (!_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void Load()
        {
            var path = GetSaveFilePath();
            if (ES3.KeyExists("DoorsLockedState", path))
                _doorsLocked = ES3.Load<Dictionary<string, DoorLockState>>("DoorsLockedState", path);

            _dirty = false;
        }


        public void OnMMEvent(DoorEvent eventType)
        {
            if (eventType.EventType == DoorEventType.Unlock) AddDoor(eventType.UniqueId, DoorLockState.Unlocked);
        }

        public bool DoorHasLockedState(string doorId)
        {
            return _doorsLocked.ContainsKey(doorId);
        }

        public DoorLockState GetDoorLockState(string doorId)
        {
            if (_doorsLocked.TryGetValue(doorId, out var lockState))
                return lockState;

            return DoorLockState.None;
        }

        void AddDoor(string doorId, DoorLockState lockState)
        {
            _doorsLocked[doorId] = lockState;
            MarkDirty();
            ConditionalSave();
        }
    }
}
