using Domains.Player.Scripts;
using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Pause
{
    public class PauseOverlay : MonoBehaviour, MMEventListener<SceneEvent>
    {
        CanvasGroup _canvasGroup;

        void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(SceneEvent eventType)
        {
            if (eventType.EventType == SceneEventType.TogglePauseScene)
            {
                var isPaused = _canvasGroup.alpha == 0;

                _canvasGroup.alpha = isPaused ? 1 : 0;
                _canvasGroup.interactable = isPaused;
                _canvasGroup.blocksRaycasts = isPaused;
                //
                // Cursor.visible = isPaused;
                // Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }

        public void DieAndReset()
        {
            // Trigger the event to reset the player
            PlayerStatusEvent.Trigger(PlayerStatusEventType.ResetManaully);
            // Optionally, you can also trigger a UI event to close the pause menu
            MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
