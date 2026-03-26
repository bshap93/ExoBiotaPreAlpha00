using System.Collections;
using Helpers.Events.NPCs;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.FeedbackControllers
{
    public class DialogueFeedbackController : MonoBehaviour, IFeedbackController, MMEventListener<DialogueCameraEvent>
    {
        [Header("Screen Fade Settings")] [SerializeField]
        CanvasGroup fadeCanvasGroup;
        [SerializeField] Image fadeImage;
        [Header("Timing")] [SerializeField] float delayBeforeFade = 0.3f; // Brief moment to process what happened
        [SerializeField] float fadeDuration = 1f;


        void Awake()
        {
            if (fadeCanvasGroup == null || fadeImage == null)
            {
                Debug.LogError(gameObject.name + ": Can't find CanvasGroup and Image");
                return;
            }

            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
        public void OnEnable()
        {
            this.MMEventStartListening();
        }
        public void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(DialogueCameraEvent eventType)
        {
            switch (eventType.Type)
            {
                case DialogueCameraEventType.FocusOnTarget:
                    break;
                case DialogueCameraEventType.ReleaseFocus:
                    break;
                case DialogueCameraEventType.FadeOut:
                    StartCoroutine(FadeOutScreen());
                    break;
                case DialogueCameraEventType.FadeIn:
                    StartCoroutine(FadeInScreen());
                    break;
            }
        }

        IEnumerator FadeOutScreen()
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;


            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        IEnumerator FadeInScreen()
        {
            if (fadeCanvasGroup == null) yield break;

            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }
}
