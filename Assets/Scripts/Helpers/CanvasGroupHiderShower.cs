using UnityEngine;

namespace Helpers
{
    [DisallowMultipleComponent]
    public class CanvasGroupHiderShower : MonoBehaviour

    {
        [SerializeField] bool startHidden = true;
        CanvasGroup _canvasGroup;
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void Start()
        {
            if (startHidden)
                Hide();
            else
                Show();
        }
        public void Hide()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
