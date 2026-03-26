using System;
using Helpers.Events.PlayerData;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.PlayerDataManagers
{
    public class JournalEntryManager : MonoBehaviour, ICoreGameService, MMEventListener<JournalEntryEvent>
    {
        [SerializeField] bool autoSave;


        bool _dirty;

        string _savePath;

        public static JournalEntryManager Instance { get; private set; }


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
        }
        public void Load()
        {
            var path = GetSaveFilePath();
        }
        public void Reset()
        {
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.JournalEntrySave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(JournalEntryEvent eventType)
        {
            throw new NotImplementedException();
        }
    }
}
