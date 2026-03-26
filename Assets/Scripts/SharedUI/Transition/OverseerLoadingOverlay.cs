using UnityEngine;

namespace SharedUI.Transition
{
    public class OverseerLoadingOverlay : MonoBehaviour
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

        public void ShowLoadingOverlay()
        {
            loadingOverlayCanvasGroup.alpha = 1;
            loadingOverlayCanvasGroup.interactable = true;
            loadingOverlayCanvasGroup.blocksRaycasts = true;
        }
    }
}
