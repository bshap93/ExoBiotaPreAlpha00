using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.Settings
{
    public class GlobalSettingsManager : MonoBehaviour, ICoreGameService, MMEventListener<GlobalSettingsEvent>
    {
        [SerializeField] bool autoSave; // checkpoint-only by default

        [Header("Initial Settings")] [SerializeField]
        FullScreenMode initialFullScreenMode = FullScreenMode.FullScreenWindow;
        [SerializeField] bool initialIsFullScreen = true;
        [SerializeField] ResolutionSettings initialResolutionSettings = new() { width = 1920, height = 1080 };
        [SerializeField] bool initialDitheringEnabled = true;
        [SerializeField] float minFieldOfView = 35f;
        [SerializeField] float maxFieldOfView = 55f;
        [SerializeField] float initialFieldOfView = 40;

        [FormerlySerializedAs("intitialMouseSensitivity")] [Range(0.1f, 2.0f)] [SerializeField]
        float intitialMouseXSensitivity = 1.0f;
        [Range(0.1f, 2.0f)] [SerializeField] float initialMouseYSensitivity = 1.0f;
        // [SerializeField] int numInputBehaviors = 4;
        [SerializeField] bool initialIsAutoSaveAtCheckpoints;
        [SerializeField] bool overrideAutoSaveAtCheckpoints;
        [SerializeField] bool initialIsCheatSheetOn;


        public List<ResolutionSettings> chooseableResolutions = new()
        {
            // 16:9 Aspect Ratio Resolutions
            new ResolutionSettings { width = 3840, height = 2160 },
            new ResolutionSettings { width = 2560, height = 1440 },
            new ResolutionSettings { width = 1920, height = 1080 },
            new ResolutionSettings { width = 1600, height = 900 },
            new ResolutionSettings { width = 1366, height = 768 },
            new ResolutionSettings { width = 1280, height = 720 },

            // 16:10 Aspect Ratio Resolutions
            new ResolutionSettings { width = 2560, height = 1600 },
            new ResolutionSettings { width = 1920, height = 1200 },
            new ResolutionSettings { width = 1680, height = 1050 },
            new ResolutionSettings { width = 1440, height = 900 },
            new ResolutionSettings { width = 1280, height = 800 },

            // 4:3 Aspect Ratio Resolutions
            new ResolutionSettings { width = 1600, height = 1200 },
            new ResolutionSettings { width = 1400, height = 1050 },
            new ResolutionSettings { width = 1024, height = 768 },

            // 21:9 Aspect Ratio Resolutions
            new ResolutionSettings { width = 3440, height = 1440 },
            new ResolutionSettings { width = 2560, height = 1080 }
        };
        readonly RefreshRate initialRefreshRate = new() { numerator = 60, denominator = 1 };
        bool _dirty;
        FullScreenMode _fullScreenMode;
        bool _isControlCheatsheetOn;
        bool _isFullScreen;
        Player _player;

        RefreshRate _refreshRate;

        ResolutionSettings _resolutionSettings;

        string _savePath;
        public static GlobalSettingsManager Instance { get; private set; }

        public bool DitheringEnabled { get; private set; }
        public float MouseXSensitivity { get; set; }
        public float MouseYSensitivity { get; set; }

        public bool IsControlCheatsheetOn
        {
            get => _isControlCheatsheetOn;
            set
            {
                _isControlCheatsheetOn = value;
                MarkDirty();
                ConditionalSave();
            }
        }

        public bool IsTutorialOn { get; private set; }

        public bool AutoSaveAtCheckpoints { get; private set; }
        public float FieldOfView { get; set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("[PlayerSaveManager] No save file found, forcing initial save...");
                Reset();
            }

            _player = ReInput.players.GetPlayer(0); // Your Player 0 usually


            Load();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        // Update is called once per frame
        public void Save()
        {
            var savePath = GetSaveFilePath();

            ES3.Save("FullScreenMode", _fullScreenMode, savePath);
            ES3.Save("IsFullScreen", _isFullScreen, savePath);
            ES3.Save("RefreshRate", _refreshRate, savePath);
            ES3.Save("ResolutionSettings", _resolutionSettings, savePath);
            ES3.Save("DitheringEnabled", DitheringEnabled, savePath);
            ES3.Save("MouseXSensitivity", MouseXSensitivity, savePath);
            ES3.Save("MouseYSensitivity", MouseYSensitivity, savePath);
            ES3.Save("TutorialOn", IsTutorialOn, savePath);
            ES3.Save("AutoSaveAtCheckpoints", AutoSaveAtCheckpoints, savePath);
            ES3.Save("IsControlCheatsheetOn", _isControlCheatsheetOn, savePath);
            ES3.Save("FieldOfView", FieldOfView, savePath);
        }
        public void Load()
        {
            var savePath = GetSaveFilePath();

            _player = ReInput.players.GetPlayer(0);

            if (!ES3.KeyExists("FullScreenMode", savePath)) return;

            if (ES3.KeyExists("FullScreenMode", savePath))
                _fullScreenMode = ES3.Load<FullScreenMode>("FullScreenMode", savePath);

            if (ES3.KeyExists("IsFullScreen", savePath))
                _isFullScreen = ES3.Load<bool>("IsFullScreen", savePath);

            if (ES3.KeyExists("IsControlCheatsheetOn", savePath))
                _isControlCheatsheetOn = ES3.Load<bool>("IsControlCheatsheetOn", savePath);
            else
                _isControlCheatsheetOn = initialIsCheatSheetOn;

            if (ES3.KeyExists("FieldOfView", savePath))
                FieldOfView = ES3.Load<float>("FieldOfView", savePath);
            else
                FieldOfView = initialFieldOfView;
            
            
            if (ES3.KeyExists("RefreshRate", savePath))
                _refreshRate = ES3.Load<RefreshRate>("RefreshRate", savePath);

            if (ES3.KeyExists("ResolutionSettings", savePath))
                _resolutionSettings = ES3.Load<ResolutionSettings>("ResolutionSettings", savePath);

            Screen.SetResolution(_resolutionSettings.width, _resolutionSettings.height, _fullScreenMode);


            if (ES3.KeyExists("DitheringEnabled", savePath))
                DitheringEnabled = ES3.Load<bool>("DitheringEnabled", savePath);

            if (ES3.KeyExists("MouseXSensitivity", savePath))
            {
                MouseXSensitivity = ES3.Load<float>("MouseXSensitivity", savePath);
                MouseXSensitivity = Mathf.Clamp(MouseXSensitivity, 0.1f, 2.0f);

                // Apply to ReInput immediately
                var xIndex = ReInput.mapping.GetInputBehaviorId("MouseX");
                var inputXMouse = ReInput.mapping.GetInputBehavior(_player.id, xIndex);
                if (inputXMouse != null)
                    inputXMouse.mouseXYAxisSensitivity = MouseXSensitivity;
            }

            if (ES3.KeyExists("MouseYSensitivity", savePath))
            {
                MouseYSensitivity = ES3.Load<float>("MouseYSensitivity", savePath);
                MouseYSensitivity = Mathf.Clamp(MouseYSensitivity, 0.1f, 2.0f);

                // Apply to ReInput immediately
                var yIndex = ReInput.mapping.GetInputBehaviorId("MouseY");
                var inputYMouse = ReInput.mapping.GetInputBehavior(_player.id, yIndex);
                if (inputYMouse != null)
                    inputYMouse.mouseXYAxisSensitivity = MouseYSensitivity;
            }

            if (ES3.KeyExists("TutorialOn", savePath))
            {
                IsTutorialOn = ES3.Load<bool>("TutorialOn", savePath);
                if (TutorialManager.Instance != null)
                    TutorialManager.Instance.SetTutorialsEnabled(IsTutorialOn);
            }

            if (ES3.KeyExists("AutoSaveAtCheckpoints", savePath))
                AutoSaveAtCheckpoints = ES3.Load<bool>("AutoSaveAtCheckpoints", savePath);
            else
                AutoSaveAtCheckpoints = initialIsAutoSaveAtCheckpoints;

            if (overrideAutoSaveAtCheckpoints) AutoSaveAtCheckpoints = initialIsAutoSaveAtCheckpoints;
        }
        public void Reset()
        {
            _fullScreenMode = initialFullScreenMode;
            _isFullScreen = initialIsFullScreen;
            _resolutionSettings = new ResolutionSettings
            {
                width = initialResolutionSettings.width,
                height = initialResolutionSettings.height
            };

            _refreshRate = new RefreshRate
            {
                numerator = initialRefreshRate.numerator,
                denominator = initialRefreshRate.denominator
            };

            DitheringEnabled = initialDitheringEnabled;
            
            FieldOfView = initialFieldOfView;

            MouseXSensitivity = intitialMouseXSensitivity;
            MouseYSensitivity = initialMouseYSensitivity;

            // Apply to ReInput immediately
            var xIndex = ReInput.mapping.GetInputBehaviorId("MouseX");
            var inputXMouse = ReInput.mapping.GetInputBehavior(_player.id, xIndex);
            if (inputXMouse != null)
                inputXMouse.mouseXYAxisSensitivity = MouseXSensitivity;

            var yIndex = ReInput.mapping.GetInputBehaviorId("MouseY");
            var inputYMouse = ReInput.mapping.GetInputBehavior(_player.id, yIndex);
            if (inputYMouse != null)
                inputYMouse.mouseXYAxisSensitivity = MouseYSensitivity;

            IsTutorialOn = true;
            TutorialManager.Instance.SetTutorialsEnabled(IsTutorialOn);

            IsControlCheatsheetOn = initialIsCheatSheetOn;

            AutoSaveAtCheckpoints = initialIsAutoSaveAtCheckpoints;


            MarkDirty();
            ConditionalSave();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.GlobalSettingsSave);
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
        public void OnMMEvent(GlobalSettingsEvent eventType)
        {
            switch (eventType.EventType)
            {
                case GlobalSettingsEventType.ResolutionChanged:
                    ChangeResolution(chooseableResolutions[eventType.ChoiceIndex]);
                    break;
                case GlobalSettingsEventType.DitheringToggled:
                    ChangeDithering(eventType.ChoiceIndex == 1);
                    break;
                case GlobalSettingsEventType.MouseXSensitivityChanged:
                    ChangeXMouseSensitivity(eventType.FloatValue);
                    break;
                case GlobalSettingsEventType.MouseYSensitivityChanged:
                    ChangeYMouseSensitivity(eventType.FloatValue);
                    break;
                case GlobalSettingsEventType.TutorialOnChanged:
                    ChangeTutorialOnOff(eventType.ChoiceIndex == 0);
                    break;
                case GlobalSettingsEventType.AutoSaveAtCheckpointsChanged:
                    ChangeAutoSaveAtCheckpoints(eventType.ChoiceIndex == 0);
                    break;
                case GlobalSettingsEventType.FullScreenModeChanged:
                    ChangeFullScreenMode((FullScreenMode)eventType.ChoiceIndex);
                    break;
                case GlobalSettingsEventType.FieldOfViewChanged:
                    ChangeFieldOfView(eventType.FloatValue);
                    break;
            }
        }
        void ChangeAutoSaveAtCheckpoints(bool checkpointAutoSaveOn)
        {
            AutoSaveAtCheckpoints = checkpointAutoSaveOn;

            MarkDirty();
            ConditionalSave();
        }

        void ChangeTutorialOnOff(bool tutorialOn)
        {
            IsTutorialOn = tutorialOn;
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.SetTutorialsEnabled(tutorialOn);

            MarkDirty();
            ConditionalSave();
        }

        void ChangeFieldOfView(float newFOV)
        {
            FieldOfView = newFOV;

            MarkDirty();
            ConditionalSave();
        }

        void ChangeXMouseSensitivity(float newSensitivity)
        {
            MouseXSensitivity = newSensitivity;

            var xIndex = ReInput.mapping.GetInputBehaviorId("MouseX");


            var inputXMouse =
                ReInput.mapping.GetInputBehavior(_player.id, xIndex);

            if (inputXMouse != null) inputXMouse.mouseXYAxisSensitivity = MouseXSensitivity;


            MarkDirty();
            ConditionalSave();
        }

        void ChangeYMouseSensitivity(float newSensitivity)
        {
            MouseYSensitivity = newSensitivity;

            var yIndex = ReInput.mapping.GetInputBehaviorId("MouseY");

            var inputYMouse =
                ReInput.mapping.GetInputBehavior(_player.id, yIndex);

            if (inputYMouse != null) inputYMouse.mouseXYAxisSensitivity = MouseYSensitivity;

            MarkDirty();
            ConditionalSave();
        }


        public void ChangeResolution(ResolutionSettings newResolution)
        {
            _resolutionSettings = newResolution;
            Debug.Log($"Setting resolution to {_resolutionSettings.width}x{_resolutionSettings.height}");
            Screen.SetResolution(
                _resolutionSettings.width, _resolutionSettings.height, _fullScreenMode);

            MarkDirty();
            ConditionalSave();
        }

        public void ChangeFullScreenMode(FullScreenMode newMode)
        {
            _fullScreenMode = newMode;
            _isFullScreen = newMode != FullScreenMode.Windowed;
            Screen.SetResolution(
                _resolutionSettings.width, _resolutionSettings.height, _fullScreenMode);

            MarkDirty();
            ConditionalSave();
        }

        public void ChangeDithering(bool ditheringEnabled)
        {
            DitheringEnabled = ditheringEnabled;

            MarkDirty();
            ConditionalSave();
        }

        [Serializable]
        public class ResolutionSettings
        {
            public int width;
            public int height;
        }
    }
}
