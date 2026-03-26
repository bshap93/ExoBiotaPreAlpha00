using System;
using System.Collections;
using System.Collections.Generic;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Helpers.Interfaces;
using Manager.SceneManagers;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.Global
{
    public class BioSamplesManager : MonoBehaviour, MMEventListener<BioSampleEvent>, ICoreGameService
    {
        static Dictionary<string, BioOrganismType> _bioDB;
        [SerializeField] bool autoSave; // checkpoint-only by default

        [SerializeField] BioOrganismManager bioOrganismManager;

        [SerializeField] [Range(1, 50)] int initialMaxNumberOfSamplesCarried = 10;

        [SerializeField] Sprite sampleGenericIcon;

        [SerializeField] AudioClip sampleGenericSound;

        [SerializeField] AudioClip sampleSequenceSound;

        [SerializeField] AudioClip failedSampleSequenceSound;

        [SerializeField] Sprite failedAnalysisIcon;

        readonly List<BioOrganismSample> _samplesCarried = new();

        readonly List<BioOrganismSample> _samplesSequencedAndLogged = new();

        bool _dirty;

        int _maxNumberOfSamplesCarried;
        string _savePath;


        public static BioSamplesManager Instance { get; private set; }

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

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[BioSamplesManager] No save file found, forcing initial save...");
                Reset(); // Ensure default values are set
            }

            Load();
        }


        public void OnEnable()
        {
            this.MMEventStartListening();
        }

        public void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("SamplesCarried", _samplesCarried, path);
            ES3.Save("SamplesSequencedAndLogged", _samplesSequencedAndLogged, path);
            ES3.Save("MaxSamplesCarried", _maxNumberOfSamplesCarried, path);
            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();
            _samplesCarried.Clear();
            if (ES3.KeyExists("SamplesCarried", path))
            {
                var loadedSamples = ES3.Load<List<BioOrganismSample>>("SamplesCarried", path);
                if (loadedSamples != null)
                    _samplesCarried.AddRange(loadedSamples);

                foreach (var sample in _samplesCarried)
                    sample.parentOrgamism = bioOrganismManager.GetBioOrganismByID(sample.parentOrganismID);
            }
            else
            {
                Debug.LogWarning("No 'SamplesCarried' key found in save file.");
            }

            if (ES3.KeyExists("MaxSamplesCarried", path))
            {
                _maxNumberOfSamplesCarried = ES3.Load<int>("MaxSamplesCarried", path);
            }
            else
            {
                Debug.LogWarning("No 'MaxSamplesCarried' key found in save file. Using default value.");
                _maxNumberOfSamplesCarried = initialMaxNumberOfSamplesCarried;
            }

            if (ES3.KeyExists("SamplesSequencedAndLogged", path))
            {
                var loadedLoggedSamples = ES3.Load<List<BioOrganismSample>>("SamplesSequencedAndLogged", path);
                if (loadedLoggedSamples != null)
                    _samplesSequencedAndLogged.AddRange(loadedLoggedSamples);
            }
            else
            {
                Debug.LogWarning("No 'SamplesSequencedAndLogged' key found in save file.");
            }

            // ResolveSampleParents(); // <-- important


            _dirty = false;
        }


        public void Reset()
        {
            _samplesCarried.Clear();
            _maxNumberOfSamplesCarried = initialMaxNumberOfSamplesCarried;
            _samplesSequencedAndLogged.Clear();
            MarkDirty();

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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.BioSamplesSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }


        public void OnMMEvent(BioSampleEvent eventType)
        {
            if (eventType.EventType == BioSampleEventType.CompleteCollection)
            {
                // var bioSample = new BioOrganismSample();
                var bioSample = new BioOrganismSample
                {
                    parentOrgamism = eventType.BioOrganismType,
                    parentOrganismID = eventType.BioOrganismType.organismID, // <-- save-friendly
                    uniqueID = eventType.UniqueID
                };

                // bioSample.parentOrgamism = eventType.BioOrganismType;
                // bioSample.uniqueID = eventType.UniqueID;
                AddSampleToPlayerCarried(bioSample);
            }

            if (eventType.EventType == BioSampleEventType.StartSequencing)
            {
                var sample = _samplesCarried.Find(s => s.uniqueID == eventType.UniqueID);
                if (sample != null)
                    TrySequenceAndLogSample(sample);
                else
                    Debug.LogWarning("Cannot sequence and log sample: sample not found in carried list.");
            }

            if (eventType.EventType == BioSampleEventType.GiveToNPC)
            {
                RemoveAllSamplesFromPlayerCarried();
                AlertEvent.Trigger(
                    AlertReason.SampleChangeHands, "You give all your samples to the NPC.", "Samples Given",
                    AlertType.Basic, 3f, sampleGenericIcon);
            }

            if (eventType.EventType == BioSampleEventType.CompletedSequencing)
            {
            }
        }

        bool HasSavedSamplesCarriedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath()) &&
                   ES3.KeyExists("SamplesCarried", _savePath ?? GetSaveFilePath());
        }

        bool HasSavedMaxSamplesCarriedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath()) &&
                   ES3.KeyExists("MaxSamplesCarried", _savePath ?? GetSaveFilePath());
        }

        bool HasSavedSamplesSequencedAndLoggedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath()) &&
                   ES3.KeyExists("SamplesSequencedAndLogged", _savePath ?? GetSaveFilePath());
        }

        bool AddSampleToPlayerCarried(BioOrganismSample sample)
        {
            if (_samplesCarried.Count >= _maxNumberOfSamplesCarried)
            {
                Debug.LogWarning("Cannot add sample: maximum capacity reached.");
                AlertEvent.Trigger(
                    AlertReason.SampleLimitExceeded,
                    "Cannot collect sample: maximum capacity reached.", "Sample Cache Full",
                    AlertType.Basic, 3f, sampleGenericIcon, sampleGenericSound);

                return false;
            }

            _samplesCarried.Add(sample);

            BioSampleEvent.Trigger(sample.uniqueID, BioSampleEventType.RefreshUI, sample.parentOrgamism, 0f);
            MarkDirty();
            ConditionalSave();
            return true;
        }

        bool TrySequenceAndLogSample(BioOrganismSample sample)
        {
            if (CanSampleAndSequenceWithCurrentGear(sample) && CanSampleAndSequenceWithCurrentSkill(sample))
            {
                StartCoroutine(SequenceAndLogSampleCoroutine(sample));
                MarkDirty();
                ConditionalSave();
                return true;
            }

            if (CanSampleAndSequenceWithCurrentGear(sample))
            {
                Debug.LogWarning("Cannot sequence and log sample: insufficient skill.");
                AlertEvent.Trigger(
                    AlertReason.CannotSequenceSample,
                    "Cannot sequence and log sample: insufficient skill.", "Insufficient Skill",
                    AlertType.Basic, 3f, failedAnalysisIcon, failedSampleSequenceSound);

                return false;
            }

            if (CanSampleAndSequenceWithCurrentSkill(sample))
            {
                Debug.LogWarning("Cannot sequence and log sample: insufficient gear.");
                AlertEvent.Trigger(
                    AlertReason.CannotSequenceSample,
                    "Cannot sequence and log sample: insufficient gear.", "Insufficient Gear",
                    AlertType.Basic, 3f, failedAnalysisIcon, failedSampleSequenceSound);

                return false;
            }

            Debug.LogWarning("Cannot sequence and log sample: insufficient gear and skill.");
            AlertEvent.Trigger(
                AlertReason.CannotSequenceSample,
                "Cannot sequence and log sample: insufficient gear and skill.", "Insufficient Gear and Skill",
                AlertType.Basic, 3f, failedAnalysisIcon, failedSampleSequenceSound);

            return false;
        }

        public IEnumerator SequenceAndLogSampleCoroutine(BioOrganismSample sample)
        {
            BioSampleEvent.Trigger(
                sample.uniqueID, BioSampleEventType.StartSequencing, sample.parentOrgamism,
                sample.GetSequencingDuration());

            var elapsedTime = 0f;
            while (elapsedTime < sample.GetSequencingDuration())
            {
                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }

            _samplesSequencedAndLogged.Add(sample);


            BioSampleEvent.Trigger(sample.uniqueID, BioSampleEventType.CompletedSequencing, sample.parentOrgamism, 0f);
        }

        bool CanSampleAndSequenceWithCurrentSkill(BioOrganismSample sample)
        {
            throw new NotImplementedException();
        }

        bool CanSampleAndSequenceWithCurrentGear(BioOrganismSample sample)
        {
            throw new NotImplementedException();
        }

        bool RemoveAllSamplesFromPlayerCarried()
        {
            if (_samplesCarried.Count > 0)
            {
                _samplesCarried.Clear();
                MarkDirty();
                ConditionalSave();
                return true;
            }

            Debug.LogWarning("Cannot remove samples: no samples found in carried list.");
            return false;
        }

        bool RemoveSampleFromPlayerCarried(string sampleUniqueID)
        {
            if (HasSampleWithID(sampleUniqueID))
            {
                var sample = _samplesCarried.Find(s => s.uniqueID == sampleUniqueID);
                if (sample == null)
                {
                    Debug.LogWarning("Cannot remove sample: sample not found in carried list.");
                    return false;
                }

                _samplesCarried.Remove(sample);
                MarkDirty();
                ConditionalSave();
                return true;
            }

            Debug.LogWarning("Cannot remove sample: sample not found in carried list.");
            return false;
        }

        bool HasSampleWithID(string sampleUniqueID)
        {
            return _samplesCarried.Exists(s => s.uniqueID == sampleUniqueID);
        }

        public List<BioOrganismSample> GetSamplesCarried()
        {
            return _samplesCarried;
        }

        public bool TryGetSampleLog(BioOrganismSample sample, out BioLogFile o)
        {
            var logFile = sample.associatedBioLogFile;
            if (logFile != null)
            {
                o = logFile;
                return true;
            }

            o = null;
            return false;
        }


        #region LLMResolve

        void EnsureBioDB()
        {
            // if (_bioDB != null) return;
            // _bioDB = new Dictionary<string, BioOrganismType>(StringComparer.Ordinal);
            // Easiest: keep your BioOrganismType assets in a Resources folder
            // foreach (var so in Resources.LoadAll<BioOrganismType>(""))
            //     if (!string.IsNullOrEmpty(so.organismID))
            //         _bioDB[so.organismID] = so;
        }

        // void ResolveSampleParents()
        // {
        //     EnsureBioDB();
        //     foreach (var s in _samplesCarried)
        //         if (s != null && !string.IsNullOrEmpty(s.parentOrganismID))
        //             _ = _bioDB.TryGetValue(s.parentOrganismID, out s.parentOrgamism);
        //
        //     foreach (var s in _samplesSequencedAndLogged)
        //         if (s != null && !string.IsNullOrEmpty(s.parentOrganismID))
        //             _ = _bioDB.TryGetValue(s.parentOrganismID, out s.parentOrgamism);
        // }

        #endregion
    }
}
