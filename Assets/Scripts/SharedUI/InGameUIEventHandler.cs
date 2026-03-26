using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI
{
    public class InGameUIEventHandler : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        [SerializeField] private CanvasGroup ingameUIMainUI;

        private void Start()
        {
            if (ingameUIMainUI == null)
            {
                Debug.LogError("InGameUIEventHandler: ingameUIMainUI is not assigned.");
                return;
            }

            HideInGameMenuUI();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.InGameUI && eventType.uiActionType == UIActionType.Open)
                ShowInGameMenuUI();
            else if (eventType.uiType == UIType.InGameUI && eventType.uiActionType == UIActionType.Close)
                HideInGameMenuUI();
        }

        private void ShowInGameMenuUI()
        {
            if (ingameUIMainUI != null)
            {
                ingameUIMainUI.alpha = 1f;
                ingameUIMainUI.interactable = true;
                ingameUIMainUI.blocksRaycasts = true;
            }
        }

        private void HideInGameMenuUI()
        {
            if (ingameUIMainUI != null)
            {
                ingameUIMainUI.alpha = 0f;
                ingameUIMainUI.interactable = false;
                ingameUIMainUI.blocksRaycasts = false;
            }
        }
    }
}