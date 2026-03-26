using System;
using System.Collections;
using Helpers.Events;
using Helpers.Events.UI;
using Manager.Settings;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI
{
    public class GlobalSettingsUIController : MonoBehaviour, MMEventListener<MyUIEvent>,
        MMEventListener<LoadedManagerEvent>
    {
        [Header("UI Elements")] [SerializeField]
        CustomDropdown resolutionDropdown;

        [SerializeField] Slider fieldOfViewSlider;

        [FormerlySerializedAs("mouseSensitivitySliderComponent")] [SerializeField]
        Slider mouseXSensitivitySliderComponent;
        [SerializeField] Slider mouseYSensitivitySliderComponent;
        [SerializeField] float initialMaxMouseSensitivity = 2.0f;

        [SerializeField] MMFeedbacks onOpenFeedbacks;
        [SerializeField] MMFeedbacks onCloseFeedbacks;

        [SerializeField] Toggle controlsCheetsheetToggle;

        [SerializeField] CustomDropdown tutorialOnDropdown;

        [SerializeField] CustomDropdown fullScreenModeDropdown;


        [Header("Features Toggles")] [SerializeField]
        bool ditheringCanBeToggled;
        [SerializeField] ButtonManager ditherToggleButton;

        [SerializeField] Toggle autoSaveAtCheckpointToggle;

        CanvasGroup _canvasGroup;

        bool _isDitheringOn;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            // Initialize to invisible
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        void Start()
        {
            SetupResolutionDropdown();
            SetupDitheringButton();
            SetUpFieldOfViewSlider();
            SetupMouseSensitivitySlider();
            SetupGameplaySettings();
            SetupControlsCheetsheetToggle();
            SetupFullScreenModeDropdown();
        }

        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();

            fieldOfViewSlider.onValueChanged.AddListener(OnFieldOfViewSliderChanged);
            mouseXSensitivitySliderComponent.onValueChanged.AddListener(OnMouseXSensitivitySliderChanged);
            mouseYSensitivitySliderComponent.onValueChanged.AddListener(OnMouseYSensitivitySliderChanged);
        }

        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
            fieldOfViewSlider.onValueChanged.RemoveListener(OnFieldOfViewSliderChanged);
            mouseXSensitivitySliderComponent.onValueChanged.RemoveListener(OnMouseXSensitivitySliderChanged);
            mouseYSensitivitySliderComponent.onValueChanged.RemoveListener(OnMouseYSensitivitySliderChanged);
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
            {
                var gsm = GlobalSettingsManager.Instance;
                if (gsm != null) autoSaveAtCheckpointToggle.isOn = gsm.AutoSaveAtCheckpoints;
                // Debug.Log(
                // "GlobalSettingsUIController updated AutoSaveAtCheckpointToggle to " +
                // gsm.AutoSaveAtCheckpoints);
            }
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.GlobalSettingsPanel)
            {
                if (eventType.uiActionType == UIActionType.Open)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
                else if (eventType.uiActionType == UIActionType.Close)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
            else if (eventType.uiActionType == UIActionType.Close) // Close any open panels on other UI types
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        void SetupFullScreenModeDropdown()
        {
            if (fullScreenModeDropdown == null)
            {
                Debug.LogWarning("fullScreenModeDropdown is not assigned in the inspector.");
                return;
            }

            // These indices must match the FullScreenMode enum values:
            // 0 = ExclusiveFullScreen, 1 = FullScreenWindow, 2 = MaximizedWindow, 3 = Windowed
            // We expose only the most useful subset — adjust as needed
            fullScreenModeDropdown.items.Clear();
            fullScreenModeDropdown.CreateNewItem("Exclusive Fullscreen"); // FullScreenMode (0)
            fullScreenModeDropdown.CreateNewItem("Borderless Fullscreen"); // FullScreenWindow (1)
            fullScreenModeDropdown.CreateNewItem("Windowed"); // Windowed (3)
            fullScreenModeDropdown.SetupDropdown();

            // Map our 3-item list to actual FullScreenMode values
            var modeMap = new[]
            {
                FullScreenMode.ExclusiveFullScreen,
                FullScreenMode.FullScreenWindow,
                FullScreenMode.Windowed
            };

            // Set current selection
            var current = Screen.fullScreenMode;
            var currentIndex = Array.IndexOf(modeMap, current);
            if (currentIndex >= 0)
                fullScreenModeDropdown.SetDropdownIndex(currentIndex);

            fullScreenModeDropdown.onValueChanged.RemoveAllListeners();
            fullScreenModeDropdown.onValueChanged.AddListener(index =>
            {
                GlobalSettingsEvent.Trigger(
                    GlobalSettingsEventType.FullScreenModeChanged,
                    (int)modeMap[index]);
            });
        }

        void SetupDitheringButton()
        {
            if (ditheringCanBeToggled)
            {
                ditherToggleButton.gameObject.SetActive(true);
                var gsm = GlobalSettingsManager.Instance;
                _isDitheringOn = gsm.DitheringEnabled;

                ditherToggleButton.onClick.AddListener(OnDitheringToggleButtonPressed);
            }
            else
            {
                ditherToggleButton.gameObject.SetActive(false);
            }
        }

        void SetupControlsCheetsheetToggle()
        {
            if (controlsCheetsheetToggle != null)
            {
                var gsm = GlobalSettingsManager.Instance;
                controlsCheetsheetToggle.isOn = gsm.IsControlCheatsheetOn;

                controlsCheetsheetToggle.onValueChanged.RemoveAllListeners();
                controlsCheetsheetToggle.onValueChanged.AddListener(OnControlsCheetSheetTogglePressed);
            }
            else
            {
                Debug.LogWarning("ControlsCheetsheetToggle is not assigned in the inspector.");
            }
        }

        void SetupMouseSensitivitySlider()
        {
            var gsm = GlobalSettingsManager.Instance;
            if (gsm == null || mouseXSensitivitySliderComponent == null)
            {
                Debug.LogWarning("GlobalSettingsManager or mouseSensitivitySliderComponent missing!");
                return;
            }

            // FIX: Set minValue to 0.1f (not 0.0f) to match valid range
            mouseXSensitivitySliderComponent.minValue = 0.1f;
            mouseXSensitivitySliderComponent.maxValue = initialMaxMouseSensitivity;
            mouseXSensitivitySliderComponent.value = gsm.MouseXSensitivity;

            mouseYSensitivitySliderComponent.minValue = 0.1f;
            mouseYSensitivitySliderComponent.maxValue = initialMaxMouseSensitivity;
            mouseYSensitivitySliderComponent.value = gsm.MouseYSensitivity;
        }

        void SetUpFieldOfViewSlider()
        {
            var gsm = GlobalSettingsManager.Instance;
            if (gsm == null || fieldOfViewSlider == null)
            {
                Debug.LogWarning("GlobalSettingsManager or fieldOfViewSlider missing!");
                return;
            }

            fieldOfViewSlider.minValue = 35;
            fieldOfViewSlider.maxValue = 55;
            fieldOfViewSlider.value = gsm.FieldOfView;
        }


        void SetupResolutionDropdown()
        {
            var gsm = GlobalSettingsManager.Instance;
            if (gsm == null || resolutionDropdown == null)
            {
                Debug.LogWarning("GlobalSettingsManager or resolutionDropdown missing!");
                return;
            }

            // Clear any existing items in the dropdown
            resolutionDropdown.items.Clear();

            // Populate dropdown with resolutions
            for (var i = 0; i < gsm.chooseableResolutions.Count; i++)
            {
                var res = gsm.chooseableResolutions[i];
                var label = $"{res.width} x {res.height}";
                resolutionDropdown.CreateNewItem(label);
            }

            // Refresh UI to apply changes
            resolutionDropdown.SetupDropdown();

            // Handle value change event
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

            // Optionally set current resolution
            var current = Screen.currentResolution;
            var index = gsm.chooseableResolutions.FindIndex(r =>
                r.width == current.width && r.height == current.height);

            if (index >= 0) resolutionDropdown.SetDropdownIndex(index);
        }

        // Setup CustomDropdown with only two options: On and Off
        void SetupGameplaySettings()
        {
            var gsm = GlobalSettingsManager.Instance;
            if (gsm == null || resolutionDropdown == null)
            {
                Debug.LogWarning("GlobalSettingsManager or resolutionDropdown missing!");
                return;
            }

            // Tutorial On/Off
            if (!gsm.IsTutorialOn)
                tutorialOnDropdown.SetDropdownIndex(1); // Off
            else
                tutorialOnDropdown.SetDropdownIndex(0); // On


            tutorialOnDropdown.onValueChanged.RemoveAllListeners();
            tutorialOnDropdown.onValueChanged.AddListener(index =>
            {
                GlobalSettingsEvent.Trigger(GlobalSettingsEventType.TutorialOnChanged, index);
            });

            // Auto Save at Checkpoint Toggle
            if (autoSaveAtCheckpointToggle != null)
            {
                autoSaveAtCheckpointToggle.isOn = gsm.AutoSaveAtCheckpoints;
                autoSaveAtCheckpointToggle.onValueChanged.RemoveAllListeners();
                autoSaveAtCheckpointToggle.onValueChanged.AddListener(isOn =>
                {
                    GlobalSettingsEvent.Trigger(GlobalSettingsEventType.AutoSaveAtCheckpointsChanged, isOn ? 0 : 1);
                });
            }
        }

        void OnFieldOfViewSliderChanged(float newValue)
        {
            GlobalSettingsEvent.Trigger(GlobalSettingsEventType.FieldOfViewChanged, newValue);
        }


        void OnResolutionChanged(int index)
        {
            GlobalSettingsEvent.Trigger(GlobalSettingsEventType.ResolutionChanged, index);
        }

        void OnMouseXSensitivitySliderChanged(float newValue)
        {
            GlobalSettingsEvent.Trigger(GlobalSettingsEventType.MouseXSensitivityChanged, newValue);
        }

        void OnMouseYSensitivitySliderChanged(float newValue)
        {
            GlobalSettingsEvent.Trigger(GlobalSettingsEventType.MouseYSensitivityChanged, newValue);
        }

        public void OnExitSettingsButtonPressed()
        {
            onCloseFeedbacks?.PlayFeedbacks();
            // MyUIEvent.Trigger(UIType.GlobalSettingsPanel, UIActionType.Close);
            // PauseEvent.Trigger(PauseEventType.PauseOff);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        IEnumerator MakeCursorVisibleAndUnlocked()
        {
            // Wait for end of frame to ensure UI has closed
            // wait 1 second to ensure any transitions are complete
            yield return new WaitForSeconds(1.0f);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        void OnDitheringToggleButtonPressed()
        {
            _isDitheringOn = !_isDitheringOn;
            GlobalSettingsEvent.Trigger(GlobalSettingsEventType.DitheringToggled, _isDitheringOn ? 1 : 0);
        }

        public void OnControlsCheetSheetTogglePressed(bool isOn)
        {
            HUDOptionalUIElementEvent.Trigger(
                HUDOptionalUIElement.ControlCheetsheet, isOn
                    ? HUDOptionalUIElementEventType.Show
                    : HUDOptionalUIElementEventType.Hide);
        }
    }
}
