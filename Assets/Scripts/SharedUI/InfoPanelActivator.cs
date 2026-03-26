using System;
using Helpers.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace SharedUI
{
    public class InfoPanelActivator : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        public string uniqueID;

        [Tooltip("Prefab to show when the object is looked at.")]
        public GameObject infoPanelPrefab;

        public bool automaticallyShowOnInteract = true;
        public Canvas canvas;

        [Tooltip("Optional offset from center of screen (Canvas space).")]
        public Vector2 screenOffset = Vector2.zero;

        [Header("Feedbacks")] public MMFeedbacks hidePanelFeedbacks;

        public UnityEvent onShowPanel;

        public UnityEvent onHidePanel;

        public MMFeedbacks showPanelFeedbacks;
        // private Coroutine _hideTimerCoroutine;

        private GameObject _infoPanelInstance;

        private bool _isPanelVisible;

        private void Awake()
        {
            if (string.IsNullOrEmpty(uniqueID)) uniqueID = Guid.NewGuid().ToString(); // Generate only if unset
        }

        private void Start()
        {
            if (canvas == null)
                // Find Canvas component by tag "NotificationUI"
                canvas = GameObject.FindGameObjectWithTag("NotificationUI")?.GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            // Make sure to clean up when disabled
            HideInfoPanel();
            this.MMEventStopListening();
        }


        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiActionType == UIActionType.Close)
                HideInfoPanel();
        }


        public void ToggleInfoPanel()
        {
            if (!_isPanelVisible)
                // UnityEngine.Debug.Log("InfoPanel is not instantiated yet.");
                ShowInfoPanel();
            else
                HideInfoPanel();
        }


        public void ShowInfoPanel()
        {
            if (infoPanelPrefab == null) return;

            if (_infoPanelInstance == null)
            {
                if (canvas == null)
                {
                    Debug.LogWarning("No Canvas found in scene!");
                    return;
                }

                _infoPanelInstance = Instantiate(infoPanelPrefab, canvas.transform, false);
                // ^ `false` keeps the prefab's RectTransform exactly as it was designed
            }

            _infoPanelInstance.SetActive(true);
            _isPanelVisible = true;
            MyUIEvent.Trigger(UIType.InfoPanel, UIActionType.Open);
            showPanelFeedbacks?.PlayFeedbacks();
            onShowPanel?.Invoke();
        }

        public void HideInfoPanel()
        {
            if (_infoPanelInstance != null && _infoPanelInstance.activeSelf)
            {
                _infoPanelInstance.SetActive(false);
                _isPanelVisible = false;
                MyUIEvent.Trigger(UIType.InfoPanel, UIActionType.Close);
                hidePanelFeedbacks?.PlayFeedbacks();
                onHidePanel?.Invoke();
            }
        }
    }
}