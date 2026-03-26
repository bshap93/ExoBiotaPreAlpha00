using System.Collections.Generic;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events.Terminals;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.ProgressionMangers
{
    public class TerminalManager : MonoBehaviour, ICoreGameService, MMEventListener<MetaTerminalEvent>
    {
        [SerializeField] bool autoSave;

        [SerializeField] List<MetaTerminalInfoSO> metaTerminalInfoSOs;

        bool _dirty;

        string _savePath;
        readonly HashSet<string> _visitedMetaTerminals = new();

        public static TerminalManager Instance { get; private set; }
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
            ES3.Save("VisitedMetaTerminals", _visitedMetaTerminals, path);
        }
        public void Load()
        {
            var path = GetSaveFilePath();

            _visitedMetaTerminals.Clear();

            if (ES3.KeyExists("VisitedMetaTerminals", path))
            {
                var set = ES3.Load<HashSet<string>>("VisitedMetaTerminals", path);
                foreach (var setItem in set) _visitedMetaTerminals.Add(setItem);
            }
        }
        public void Reset()
        {
            _visitedMetaTerminals.Clear();

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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.TerminalsSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(MetaTerminalEvent eventType)
        {
            if (eventType.EventType == MetaTerminalEventType.MetaTerminalRegistered)
            {
                _visitedMetaTerminals.Add(eventType.TerminalUniqueID);
                MarkDirty();
            }
            else if (eventType.EventType == MetaTerminalEventType.RequestedFastTravelToOtherTerminal)
            {
                // Handle fast travel request if needed
            }
        }


        public List<string> GetMetaTerminalNames()
        {
            var names = new List<string>();
            foreach (var metaTerminalInfoSO in metaTerminalInfoSOs) names.Add(metaTerminalInfoSO.terminalName);

            return names;
        }
    }
}
