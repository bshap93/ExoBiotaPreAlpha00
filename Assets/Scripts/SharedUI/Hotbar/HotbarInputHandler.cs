using Helpers.Events;
using Manager;
using Manager.DialogueScene;
using Manager.Global;
using MoreMountains.Tools;
using UnityEngine;
using Utilities.Inputs;

namespace SharedUI.Hotbar
{
    /// <summary>
    ///     Connects the RewiredFirstPersonInputs to the Hotbar system
    /// </summary>
    [RequireComponent(typeof(RewiredFirstPersonInputs))]
    public class HotbarInputHandler : MonoBehaviour, MMEventListener<LoadedManagerEvent>
    {
        [SerializeField] FPHUDHotbars fpHudHotbars;

        [Header("Mouse Wheel Settings")] [SerializeField]
        bool enableMouseWheelToolCycling = true;
        [SerializeField] float scrollThreshold = 0.1f; // Minimum scroll amount to register
        [SerializeField] float scrollCooldown = 0.2f; // Cooldown between scroll actions
        DialogueManager _dialogueManager;

        RewiredFirstPersonInputs _inputs;
        float _lastScrollTime;
        PauseManager _pauseManager;
        PlayerUIManager _playerUiManager;

        void Start()
        {
            _inputs = GetComponent<RewiredFirstPersonInputs>();

            if (_inputs == null) Debug.LogError("[HotbarInputHandler] RewiredFirstPersonInputs component not found!");

            if (fpHudHotbars == null)
            {
                fpHudHotbars = FindFirstObjectByType<FPHUDHotbars>();
                if (fpHudHotbars == null) Debug.LogError("[HotbarInputHandler] FPHUDHotbars not found in scene!");
            }
        }

        void Update()
        {
            if (_inputs == null || fpHudHotbars == null) return;

            // Handle mouse wheel tool cycling
            if (enableMouseWheelToolCycling) HandleMouseWheelCycling();

            // Check each hotbar key
            if (_inputs.hotbarFP1)
                fpHudHotbars.HandleHotbarKeyPress(1);
            else if (_inputs.hotbarFP2)
                fpHudHotbars.HandleHotbarKeyPress(2);
            else if (_inputs.hotbarFP3)
                fpHudHotbars.HandleHotbarKeyPress(3);
            else if (_inputs.hotbarFP4)
                fpHudHotbars.HandleHotbarKeyPress(4);
            else if (_inputs.hotbarFP5)
                fpHudHotbars.HandleHotbarKeyPress(5);
            else if (_inputs.hotbarFP6) fpHudHotbars.HandleHotbarKeyPress(6);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
            {
                _pauseManager = PauseManager.Instance;
                _playerUiManager = PlayerUIManager.Instance;
                _dialogueManager = DialogueManager.Instance;
            }
        }

        void HandleMouseWheelCycling()
        {
            if (_pauseManager != null && _pauseManager.IsPaused()) return;
            if (_playerUiManager != null && _playerUiManager.iGUIsOpen) return;
            if (_dialogueManager != null && _dialogueManager.IsDialogueActive) return;
            var scrollDelta = _inputs.scrollBetweenTools;

            // Check if enough time has passed since last scroll and scroll amount is significant
            if (Mathf.Abs(scrollDelta) < scrollThreshold || Time.time - _lastScrollTime < scrollCooldown) return;

            _lastScrollTime = Time.time;

            if (scrollDelta > 0)
                // Scroll up - next tool
                fpHudHotbars.CycleToolHotbar(1);
            else if (scrollDelta < 0)
                // Scroll down - previous tool
                fpHudHotbars.CycleToolHotbar(-1);
        }
    }
}
