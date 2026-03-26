using System.Collections.Generic;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.ProgressionMangers
{
    public class CheckpointManager : MonoBehaviour, ICoreGameService, MMEventListener<CheckpointEvent>
    {
        [SerializeField] bool autoSave;

        readonly HashSet<string> _visitedCheckpoints = new();
        bool _dirty;
        string _savePath;

        public static CheckpointManager Instance { get; private set; }

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
            ES3.Save("VisitedCheckpoints", _visitedCheckpoints, path);
        }
        public void Load()
        {
            var path = GetSaveFilePath();

            _visitedCheckpoints.Clear();
            if (ES3.KeyExists("VisitedCheckpoints", path))
            {
                var set = ES3.Load<HashSet<string>>("VisitedCheckpoints", path);
                foreach (var checkpoint in set) _visitedCheckpoints.Add(checkpoint);
            }
        }
        public void Reset()
        {
            _visitedCheckpoints.Clear();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.CheckpointsSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(CheckpointEvent eventType)
        {
            if (eventType.CheckpointEventType == CheckpointEventType.Visited)
                if (_visitedCheckpoints.Add(eventType.UniqueCheckpointId))
                {
                    MarkDirty();
                    ConditionalSave();
                }
        }

        public bool HasCheckpointBeenVisited(string uniqueCheckpointId)
        {
            return _visitedCheckpoints.Contains(uniqueCheckpointId);
        }
    }
}
