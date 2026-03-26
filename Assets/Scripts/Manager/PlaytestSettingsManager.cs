using Helpers.Events;
using Helpers.Events.Playtest;
using Helpers.Interfaces;
using Helpers.ScriptableObjects;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager
{
    public class PlaytestSettingsManager : MonoBehaviour, ICoreGameService, MMEventListener<PlaytestInfoLogEvent>
    {
        [SerializeField] bool initialIsPlaytestValue;
        [ShowIf("initialIsPlaytestValue")] [SerializeField]
        string initialPlaytestVersion;

        [SerializeField] bool showInitialInfoLogContent;
        [ShowIf("showInitialInfoLogContent")] [SerializeField]
        InfoLogContent openingPlaytestInfoLogContent;
        [SerializeField] bool autoSave; // checkpoint-only by default
        bool _dirty;


        string _savePath;

        public bool HasShownPlaytestInfoLog { get; private set; }

        public bool IsPlayTest { get; private set; }
        public string PlaytestVersion { get; private set; }
        public static PlaytestSettingsManager Instance { get; private set; }

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
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("No existing playtest settings save file found. Initializing with default values.");
                Reset();
            }

            IsPlayTest = initialIsPlaytestValue;
            PlaytestVersion = initialPlaytestVersion;


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
            var savePath = GetSaveFilePath();

            ES3.Save("HasShownPlaytestInfoLog", HasShownPlaytestInfoLog, savePath);
        }
        public void Load()
        {
            var savePath = GetSaveFilePath();
            if (ES3.KeyExists("HasShownPlaytestInfoLog", savePath))
                HasShownPlaytestInfoLog = ES3.Load<bool>("HasShownPlaytestInfoLog", savePath);
            else
                HasShownPlaytestInfoLog = false;
        }
        public void Reset()
        {
            HasShownPlaytestInfoLog = false;
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.PlaytestSettingsSave);
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
        public void OnMMEvent(PlaytestInfoLogEvent eventType)
        {
            if (eventType.Type == PlaytestInfoLogEventType.Intro && IsPlayTest && showInitialInfoLogContent)
            {
                ShowLogAfterDelay();
                Debug.Log("Triggered playtest info log event: " + eventType.Type);
            }
        }


        void ShowLogAfterDelay()
        {
            if (HasShownPlaytestInfoLog) return;
            InfoLogEvent.Trigger(openingPlaytestInfoLogContent, InfoLogEventType.SetInfoLogContent);
            MyUIEvent.Trigger(UIType.InfoLogTablet, UIActionType.Open);

            HasShownPlaytestInfoLog = true;
            MarkDirty();
            ConditionalSave();
        }
    }
}
