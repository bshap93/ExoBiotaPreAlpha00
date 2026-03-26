using Helpers.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Rewired;
using TMPro;
using UnityEngine;

namespace SharedUI.Interact
{
    public class InfoLogTabletUI : MonoBehaviour, MMEventListener<MyUIEvent>, MMEventListener<InfoLogEvent>
    {
        [SerializeField] MMFeedbacks openFeedbacks;
        [SerializeField] CanvasGroup infoLogTabletCanvasGroup;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] TMP_Text authorText;
        [SerializeField] TMP_Text dateText;
        bool _isHidden = true;

        Player _player;

        void Start()
        {
            _player = ReInput.players.GetPlayer(0);
            Hide();
        }

        void Update()
        {
            if (_isHidden) return;
            if (_player == null) return;
            var isCloseButtonPressed = _player.GetButtonDown("Interact");
            var isEscapeButtonPressed = _player.GetButtonDown("Pause");
            var isIGuiButtonPressed = _player.GetButtonDown("ToggleIGUI");
            if (isCloseButtonPressed || isEscapeButtonPressed || isIGuiButtonPressed) Quit();
        }
        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<InfoLogEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<InfoLogEvent>();
        }
        public void OnMMEvent(InfoLogEvent eventType)
        {
            if (eventType.InfoLogEventType == InfoLogEventType.SetInfoLogContent)
            {
                titleText.text = eventType.InfoLogContent.title;
                bodyText.text = eventType.InfoLogContent.body;
                authorText.text = eventType.InfoLogContent.author;
                dateText.text = eventType.InfoLogContent.dateText;
            }
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType != UIType.InfoLogTablet) return;

            switch (eventType.uiActionType)
            {
                case UIActionType.Open:
                    Show();
                    break;
                case UIActionType.Close:
                    Hide();
                    break;
            }
        }

        void Show()
        {
            infoLogTabletCanvasGroup.alpha = 1f;
            infoLogTabletCanvasGroup.blocksRaycasts = true;
            infoLogTabletCanvasGroup.interactable = true;
            _isHidden = false;
            openFeedbacks.PlayFeedbacks();
        }

        void Hide()
        {
            infoLogTabletCanvasGroup.alpha = 0f;
            infoLogTabletCanvasGroup.blocksRaycasts = false;
            infoLogTabletCanvasGroup.interactable = false;
            _isHidden = true;
        }

        public void Quit()
        {
            Hide();
            MyUIEvent.Trigger(UIType.InfoLogTablet, UIActionType.Close);
        }
    }
}
