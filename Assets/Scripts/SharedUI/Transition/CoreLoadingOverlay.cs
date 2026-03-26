using Events;
using Helpers.Events.UI;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Transition
{
    public class CoreLoadingOverlay : MonoBehaviour,
        MMEventListener<SceneTransitionUIEvent>, MMEventListener<ModeLoadEvent>
    {
        [SerializeField] CanvasGroup loadingOverlayCanvasGroup;
        [SerializeField] bool startVisible = true;

        void Start()
        {
            if (startVisible)
            {
                loadingOverlayCanvasGroup.alpha = 1;
                loadingOverlayCanvasGroup.interactable = true;
                loadingOverlayCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                loadingOverlayCanvasGroup.alpha = 0;
                loadingOverlayCanvasGroup.interactable = false;
                loadingOverlayCanvasGroup.blocksRaycasts = false;
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<SceneTransitionUIEvent>();
            this.MMEventStartListening<ModeLoadEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<SceneTransitionUIEvent>();
            this.MMEventStopListening<ModeLoadEvent>();
        }
        public void OnMMEvent(ModeLoadEvent eventType)
        {
            if (eventType.EventType == ModeLoadEventType.Load || eventType.EventType == ModeLoadEventType.Enabled)
            {
                loadingOverlayCanvasGroup.alpha = 0;
                loadingOverlayCanvasGroup.interactable = false;
                loadingOverlayCanvasGroup.blocksRaycasts = false;
            }
        }

        public void OnMMEvent(SceneTransitionUIEvent eventType)
        {
            if (eventType.EventType == SceneTransitionUIEventType.Show)
            {
                loadingOverlayCanvasGroup.alpha = 1;
                loadingOverlayCanvasGroup.interactable = true;
                loadingOverlayCanvasGroup.blocksRaycasts = true;
            }
            else if (eventType.EventType == SceneTransitionUIEventType.Hide)
            {
                loadingOverlayCanvasGroup.alpha = 0;
                loadingOverlayCanvasGroup.interactable = false;
                loadingOverlayCanvasGroup.blocksRaycasts = false;
            }
        }
    }
}
