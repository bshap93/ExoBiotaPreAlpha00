using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.Triggering;
using Helpers.Events.Tutorial;
using Helpers.Interfaces;
using Helpers.ScriptableObjects.Tutorial;
using MoreMountains.Tools;
using PhysicsHandlers.Triggers;
using UnityEngine;

namespace Manager
{
    public class TutorialManager : MonoBehaviour, ICoreGameService, MMEventListener<MainTutorialBitEvent>, MMEventListener<TriggerColliderEvent>
    {
        [SerializeField] bool autoSave; // checkpoint-only by default

        [SerializeField] AudioSource uiButtonAudioSource;
        readonly HashSet<string> _tutorialBitsCompleted = new();
        readonly HashSet<string> _dialogueInitTriggersCleared = new();
        

        List<AudioSource> _audioSources = new();
        readonly HashSet<string> _colliderTutorialTriggersCleared = new();

        bool _dirty;

        string _savePath;
        Dictionary<string, MainTutBitWindowArgs> _tutBitsById;

        bool _tutorialsEnabled = true;
        public static TutorialManager Instance { get; private set; }

        public bool IsOpen { get; private set; }


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
            LoadAllTutBits();

            if (!HasSavedData())
            {
                Reset();
                return;
            }

            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening<MainTutorialBitEvent>();
            this.MMEventStartListening<TriggerColliderEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MainTutorialBitEvent>();
            this.MMEventStopListening<TriggerColliderEvent>();
        }
        public void Save()
        {
            _savePath = GetSaveFilePath();

            ES3.Save("TutorialBitsCompleted", _tutorialBitsCompleted, _savePath);
            ES3.Save("ColliderTutorialTriggersCleared", _colliderTutorialTriggersCleared, _savePath);
            ES3.Save("DialogueInitTriggersCleared", _dialogueInitTriggersCleared, _savePath);
            _dirty = false;
        }
        public void Load()
        {
            _savePath = GetSaveFilePath();

            _tutorialBitsCompleted.Clear();
            _colliderTutorialTriggersCleared.Clear();
            _dialogueInitTriggersCleared.Clear();

            if (ES3.KeyExists("TutorialBitsCompleted", _savePath))
            {
                var set = ES3.Load<HashSet<string>>("TutorialBitsCompleted", _savePath);
                foreach (var id in set)
                    _tutorialBitsCompleted.Add(id);
            }

            if (ES3.KeyExists("ColliderTutorialTriggersCleared", _savePath))
            {
                var set = ES3.Load<HashSet<string>>("ColliderTutorialTriggersCleared", _savePath);
                foreach (var id in set)
                    _colliderTutorialTriggersCleared.Add(id);
            }
            
            if (ES3.KeyExists("DialogueInitTriggersCleared", _savePath))
            {
                var set = ES3.Load<HashSet<string>>("DialogueInitTriggersCleared", _savePath);
                foreach (var id in set)
                    _dialogueInitTriggersCleared.Add(id);
            }


            _dirty = false;
        }
        public void Reset()
        {
            _tutorialBitsCompleted.Clear();
            _colliderTutorialTriggersCleared.Clear(); 
            _dialogueInitTriggersCleared.Clear();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.TutorialSave);
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

        public void OnMMEvent(MainTutorialBitEvent bitEventType)
        {
            // if (!_tutorialsEnabled) return;
            // Check if IGUI is open
            if (PlayerUIManager.Instance.iGUIsOpen &&
                bitEventType.BitEventType == MainTutorialBitEventType.FinishTutBit)
            {
                MarkTutorialBitComplete(bitEventType.MainTutID);
                IsOpen = false;
                return;
            }

            if (bitEventType.BitEventType == MainTutorialBitEventType.FinishTutBit)
            {
                MarkTutorialBitComplete(bitEventType.MainTutID);
                MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
                Time.timeScale = 1;
                IsOpen = false;
            }

            if (bitEventType.BitEventType == MainTutorialBitEventType.ShowMainTutBit)
            {
                // PauseAudio();
                MyUIEvent.Trigger(UIType.MainTutorial, UIActionType.Open);
                IsOpen = true;
                // Time.timeScale = 0;
            }
        }

        public bool AreTutorialsEnabled()
        {
            return _tutorialsEnabled;
        }

        public void SetTutorialsEnabled(bool tutorialOn)
        {
            _tutorialsEnabled = tutorialOn;

            // If disabling mid-tutorial, close any open tutorial
            if (!tutorialOn && IsOpen)
            {
                MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
                IsOpen = false;
            }
        }

        void LoadAllTutBits()
        {
            var allBits = Resources.LoadAll<MainTutBitWindowArgs>("MainTutBits");
            _tutBitsById = new Dictionary<string, MainTutBitWindowArgs>();

            foreach (var bit in allBits)
                if (!_tutBitsById.ContainsKey(bit.mainTutID))
                    _tutBitsById.Add(bit.mainTutID, bit);
                else
                    Debug.LogWarning($"Duplicate tutorial ID found: {bit.mainTutID}");

            // Debug.Log($"Loaded {_tutBitsById.Count} tutorial bits.");
        }


        public MainTutBitWindowArgs GetTutBitById(string id)
        {
            if (_tutBitsById != null && _tutBitsById.TryGetValue(id, out var bit))
                return bit;

            Debug.LogWarning($"Tutorial bit not found for ID: {id}");
            return null;
        }

        public bool IsTutorialBitComplete(string tutBitID)
        {
            return _tutorialBitsCompleted.Contains(tutBitID);
        }

        public void MarkTutorialBitComplete(string tutBitID)
        {
            if (_tutorialBitsCompleted.Add(tutBitID))
            {
                _dirty = true;
                ConditionalSave();
            }
        }

        void UnPauseAudio()
        {
            foreach (var audioSource in _audioSources)
                if (audioSource != null && audioSource != uiButtonAudioSource)
                    audioSource.UnPause();
        }

        void PauseAudio()
        {
            _audioSources = new List<AudioSource>(FindObjectsByType<AudioSource>(FindObjectsSortMode.None));
            foreach (var audioSource in _audioSources)
                if (audioSource != null && audioSource != uiButtonAudioSource)
                    audioSource.Pause();
        }
        public bool IsTutorialOpen()
        {
            return IsOpen;
        }
        public IEnumerable<string> GetCompletedTutBits()
        {
            return _tutorialBitsCompleted;
        }

        public IEnumerable<MainTutBitWindowArgs> GetAllTutBits()
        {
            return _tutBitsById.Values;
        }
        public bool IsControlPromptSequenceComplete(string controlPromptSequenceID)
        {
            throw new NotImplementedException();
        }
        public void OnMMEvent(TriggerColliderEvent eventType)
        {
            if (eventType.ColliderType== TriggerColliderType.Dialogue)
            {
                if (eventType.EventType == TriggerColliderEventType.SetTriggerable)
                {
                    // throw new NotImplementedException();
                }
            }
            
        }
    }
}
