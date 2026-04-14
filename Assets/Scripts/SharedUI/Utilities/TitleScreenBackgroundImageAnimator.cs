using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Utilities
{
    public class TitleScreenBackgroundImageAnimator : MonoBehaviour
    {
        public Image backgroundImage;
        public float lowerBound = 2.15f;
        public float upperBound = 3.91f;
        public float duration = 2f;
        public Ease easeType = Ease.InOutSine;

        void Start()
        {
            if (backgroundImage != null) StartPingPongTween();
        }

        void OnDestroy()
        {
            DOTween.Kill(backgroundImage); // Clean up on destroy
        }

        void StartPingPongTween()
        {
            backgroundImage.pixelsPerUnitMultiplier = lowerBound;

            DOTween.To(
                    () => backgroundImage.pixelsPerUnitMultiplier,
                    x => backgroundImage.pixelsPerUnitMultiplier = x,
                    upperBound,
                    duration
                )
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo); // -1 = infinite, Yoyo = ping-pong
        }
    }
}
