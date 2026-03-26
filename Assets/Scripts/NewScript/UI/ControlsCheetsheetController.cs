using Helpers.Events.UI;
using Manager.Settings;
using MoreMountains.Tools;
using UnityEngine;

namespace NewScript.UI
{
    public class ControlsCheetsheetController : MonoBehaviour, MMEventListener<HUDOptionalUIElementEvent>
    {
        [SerializeField] GlobalSettingsManager globalSettingsManager;
        [SerializeField] CanvasGroup canvasGroup;

        bool _shown;

        void Start()
        {
            if (globalSettingsManager.IsControlCheatsheetOn)
                Show();
            else
                Hide();
        }


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(HUDOptionalUIElementEvent eventType)
        {
            if (eventType.element != HUDOptionalUIElement.ControlCheetsheet) return;

            switch (eventType.eventType)
            {
                case HUDOptionalUIElementEventType.Toggle:
                    if (_shown) Hide();
                    else Show();

                    break;
                case HUDOptionalUIElementEventType.Show:
                    Show();
                    break;
                case HUDOptionalUIElementEventType.Hide:
                    Hide();
                    break;
            }
        }

        void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            _shown = true;
        }

        void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _shown = false;
        }
    }
}
