using System;
using System.Collections.Generic;
using Helpers.Events;
using Manager.Global;
using Manager.Status.Scriptable;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using OWPData.Structs;
using Structs;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.Alert
{
    public class AlertUIController : MonoBehaviour, MMEventListener<AlertEvent>
    {
        [SerializeField] MMFeedbacks normalAlertFeedbacks;
        [FormerlySerializedAs("_modalWindowManager")] [SerializeField]
        ModalWindowManager modalWindowManager;
        [SerializeField] AudioSource uiButtonAudioSource;
        [SerializeField] PauseAndGiveInfoPanel pauseAndGiveInfoPanel;

        public List<ReasonToPauseAndGiveInfoDetails> reasonToPauseAndGiveInfoDetails;

        [Header("Alert Cooldowns")] [Tooltip("Minimum time between showing the same alert reason")] [SerializeField]
        float alertCooldownTime = 3f;
        readonly Dictionary<AlertReason, float> _lastAlertTimes = new();
        List<AudioSource> _audioSources = new();

        CanvasGroup _canvasGroup;
        NotificationManager _notificationManager;

        bool _pauseAndGiveInfoPanelIsOpen;


        void Awake()
        {
            _notificationManager = GetComponentInChildren<NotificationManager>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 1;
            // _canvasGroup.interactable = true;
            // _canvasGroup.blocksRaycasts = true;
        }


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(AlertEvent eventType)
        {
            if (eventType.AlertType == AlertType.PauseAndGiveInfo)
            {
                // Ignore if panel is already open
                if (_pauseAndGiveInfoPanelIsOpen) return;

                // Check cooldown for this specific alert reason
                if (_lastAlertTimes.TryGetValue(eventType.AlertReason, out var lastTime))
                    if (Time.realtimeSinceStartup - lastTime < alertCooldownTime)
                        return; // Still in cooldown
            }

            if (eventType.AlertType == AlertType.Basic)
            {
                ShowBasicAlert(eventType);
            }
            else if (eventType.AlertType == AlertType.ChoiceModal)
            {
                ShowChoiceModal(eventType);
            }
            else if (eventType.AlertType == AlertType.PauseAndGiveInfo)
            {
                normalAlertFeedbacks?.PlayFeedbacks();

                // ✅ FIX: Mark panel as open and update cooldown BEFORE triggering pause
                _pauseAndGiveInfoPanelIsOpen = true;
                _lastAlertTimes[eventType.AlertReason] = Time.realtimeSinceStartup;

                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Pause);
                ShowPauseAndGiveInfoPanel(eventType.AlertReason);
            }
        }

        public PauseAndGiveInfoDetails GetPauseAndGiveInfoDetails(AlertReason reason)
        {
            foreach (var mapping in reasonToPauseAndGiveInfoDetails)
                if (mapping.alertReason == reason)
                    return mapping.details;

            Debug.LogWarning($"No PauseAndGiveInfoDetails found for reason {reason}");
            return null;
        }
        void ShowPauseAndGiveInfoPanel(AlertReason eventTypeAlertReason)
        {
            var details = GetPauseAndGiveInfoDetails(eventTypeAlertReason);
            if (details == null)
            {
                Debug.LogError($"No details found for reason {eventTypeAlertReason}");
                return;
            }

            pauseAndGiveInfoPanel.Initialize(details);
            pauseAndGiveInfoPanel.Open();
        }

        public void ClosePauseAndGiveInfoPanel()
        {
            _pauseAndGiveInfoPanelIsOpen = false;
            pauseAndGiveInfoPanel.Close();
            InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Resume);
        }


        public void ShowBasicAlert(AlertEvent evt)
        {
            _notificationManager.title = evt.AlertTitle;
            _notificationManager.description = evt.AlertMessage;
            _notificationManager.icon = evt.AlertIcon;
            _notificationManager.UpdateUI();


            _notificationManager.Open();
        }

        public void ShowChoiceModal(AlertEvent evt)
        {
            _canvasGroup.alpha = 1;
            // Fill content
            modalWindowManager.titleText = evt.AlertTitle;
            modalWindowManager.descriptionText = evt.AlertMessage;
            modalWindowManager.icon = evt.AlertIcon;
            modalWindowManager.showConfirmButton = true;
            modalWindowManager.showCancelButton = true;
            modalWindowManager.UpdateUI();

            // PauseAudio();
            MyUIEvent.Trigger(UIType.ModalBoxChoice, UIActionType.Open);
            // Time.timeScale = 0;
            InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Pause);


            // Rebind listeners FOR THIS OPEN ONLY to avoid stacking
            modalWindowManager.onOK.RemoveAllListeners();
            modalWindowManager.onCancel.RemoveAllListeners();

            GameMode currentGameMode;

            if (GameStateManager.Instance == null)
            {
                Debug.LogError("GameStateManager Instance found.");
                return;
            }

            currentGameMode = GameStateManager.Instance.CurrentMode;


            modalWindowManager.onOK.AddListener(() =>
            {
                try
                {
                    evt.OnConfirm?.Invoke();
                }
                finally
                {
                    // Close + unpause
                    modalWindowManager.Close();
                    // Time.timeScale = 1;
                    // UnPauseAudio();

                    MyUIEvent.Trigger(UIType.ModalBoxChoice, UIActionType.Close);
                    InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Resume, gameMode: currentGameMode);
                }
            });

            modalWindowManager.onCancel.AddListener(() =>
            {
                try
                {
                    evt.OnCancel?.Invoke();
                }
                finally
                {
                    modalWindowManager.Close();
                    // Time.timeScale = 1;
                    // UnPauseAudio();
                    MyUIEvent.Trigger(UIType.ModalBoxChoice, UIActionType.Close);
                    InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Resume, gameMode: currentGameMode);
                }
            });

            // Open it
            modalWindowManager.Open();
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


        public void HideAlert()
        {
            _notificationManager.Close();
        }

        [Serializable]
        public struct ReasonToPauseAndGiveInfoDetails
        {
            public AlertReason alertReason;
            public PauseAndGiveInfoDetails details;
        }

        [Serializable]
        public struct ModalArgs
        {
            [FormerlySerializedAs("Description")] [TextArea(1, 4)]
            public string description;
            [FormerlySerializedAs("Icon")] public Sprite icon;
            public Action OnCancel;
            public Action OnConfirm;
            [FormerlySerializedAs("ShowCancelButton")]
            public bool showCancelButton;
            [FormerlySerializedAs("ShowConfirmButton")]
            public bool showConfirmButton;
            [FormerlySerializedAs("Title")] public string title;
            public string ID;
            public StatusEffect statusEffect;
        }
    }
}
