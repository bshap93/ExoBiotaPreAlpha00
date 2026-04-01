using System;
using System.Collections.Generic;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.SceneManagers
{
    public class BioOrganismManager : MonoBehaviour, MMEventListener<BioOrganismEvent>, ICoreGameService
    {
        [SerializeField] bool autoSave;

        readonly Dictionary<string, int> _timesLeftToSample = new(StringComparer.Ordinal);

        readonly Dictionary<string, Dictionary<string, int>> _timesLeftToSampleByScene =
            new(StringComparer.Ordinal);

        bool _dirty;

        OrganismInformation[] _organismInformationArray;

        string _savePath;

        public static BioOrganismManager Instance { get; private set; }

        #region Events

        public void OnMMEvent(BioOrganismEvent e)
        {
            // Handle events here if needed
        }

        #endregion

        public BioOrganismType GetBioOrganismByID(string sampleParentOrganismID)
        {
            foreach (var organismInformation in _organismInformationArray)
                if (organismInformation.organismId == sampleParentOrganismID)
                    return organismInformation.organismType;

            return null;
        }
        public bool IsDepleted(string sceneKey, string uniqueID)
        {
            if (GetTimesLeft(sceneKey, uniqueID, 1) < 1) return true;

            return false;
        }

        [Serializable]
        public class OrganismInformation
        {
            public string organismId;
            public BioOrganismType organismType;

            public string OrganismName => organismType.organismName;
        }

        #region LifeCycle

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad(gameObject);

            PopulateOrganismInformation();
        }

        void PopulateOrganismInformation()
        {
            // Loads all BioOrganismType assets from Resources/BioOrganismType/ AND all subfolders
            var loaded = Resources.LoadAll<BioOrganismType>("BioOrganismType");

            _organismInformationArray = new OrganismInformation[loaded.Length];

            for (var i = 0; i < loaded.Length; i++)
                _organismInformationArray[i] = new OrganismInformation
                {
                    // Uses the asset name as the ID — or add an explicit ID field to your SO
                    organismId = loaded[i].name,
                    organismType = loaded[i]
                };
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("No saved data found for BioOrganismManager.");
                Reset();
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

        #endregion

        #region IGameService

        public void Save()
        {
            var path = GetSaveFilePath();

            ES3.Save("TimesLeftToSample", _timesLeftToSample, path);
            ES3.Save("TimesLeftToSampleByScene", _timesLeftToSampleByScene, path);

            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            _timesLeftToSample.Clear();
            _timesLeftToSampleByScene.Clear();

            if (ES3.KeyExists("TimesLeftToSample", path))
            {
                var dict = ES3.Load<Dictionary<string, int>>("TimesLeftToSample", path);
                foreach (var kvp in dict) _timesLeftToSample[kvp.Key] = kvp.Value;
            }

            if (ES3.KeyExists("TimesLeftToSampleByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, Dictionary<string, int>>>("TimesLeftToSampleByScene", path);
                foreach (var kvp in dict) _timesLeftToSampleByScene[kvp.Key] = kvp.Value;
            }

            _dirty = false;
        }

// In BioOrganismManager (persistent, ES3-backed dictionaries)
        public int GetTimesLeft(string sceneName, string nodeId, int defaultAllowed)
        {
            if (!_timesLeftToSampleByScene.TryGetValue(sceneName, out var perScene))
                perScene = _timesLeftToSampleByScene[sceneName] = new Dictionary<string, int>(StringComparer.Ordinal);

            if (!perScene.TryGetValue(nodeId, out var left))
            {
                left = Mathf.Max(0, defaultAllowed);
                perScene[nodeId] = left;
                MarkDirty();
                ConditionalSave();
            }

            return left;
        }

        public bool ConsumeOne(string sceneName, string nodeId)
        {
            if (!_timesLeftToSampleByScene.TryGetValue(sceneName, out var perScene) ||
                !perScene.TryGetValue(nodeId, out var left) || left <= 0)
                return false;

            perScene[nodeId] = left - 1;
            MarkDirty();
            ConditionalSave();
            return true;
        }


        public void Reset()
        {
            _timesLeftToSample.Clear();
            _timesLeftToSampleByScene.Clear();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.BioOrganismSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        #endregion
    }
}
