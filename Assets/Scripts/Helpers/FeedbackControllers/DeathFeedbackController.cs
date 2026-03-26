using System.Collections;
using Helpers.Events;
using Helpers.Events.Death;
using Helpers.Interfaces;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.FeedbackControllers
{
    public class DeathFeedbackController : MonoBehaviour, IFeedbackController,
        MMEventListener<PlayerDeathEvent>
    {
        [Header("Screen Fade Settings")] [SerializeField]
        CanvasGroup fadeCanvasGroup;
        [SerializeField] Image fadeImage;
        [SerializeField] float fadeDuration = 2f;
        [SerializeField] AudioSource globalAmbientAS;
        [SerializeField] AudioSource otherEnvironmentalAS;
        [SerializeField] AudioListener playerAudioListener;

        [SerializeField] bool volumeShouldFade;
        // [Header("Camera Settings")]
        // [SerializeField] private Transform cameraTransform;
        // [SerializeField] private float deathShakeIntensity = 0.3f;
        // [SerializeField] private float deathShakeDuration = 0.5f;
        // [SerializeField] private float cameraTiltAmount = 15f;
        // [SerializeField] private float cameraTiltDuration = 1.5f;

        [SerializeField] MMFeedbacks additionalFeedbacksOnDeath;

        [Header("Audio Settings")] [SerializeField]
        float audioFadeOutDuration = 1.5f;
        [SerializeField] AnimationCurve audioFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Timing")] [SerializeField] float delayBeforeFade = 0.3f; // Brief moment to process what happened
        [SerializeField] float delayBeforeSceneLoad = 0.5f; // Pause at full black before loading

        bool _isProcessingDeath;
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

        public void OnMMEvent(PlayerDeathEvent eventType)
        {
            if (!_isProcessingDeath) StartCoroutine(HandleDeathSequence());
        }

        IEnumerator HandleDeathSequence()
        {
            _isProcessingDeath = true;
            additionalFeedbacksOnDeath?.PlayFeedbacks();

            yield return new WaitForSeconds(delayBeforeFade);

            StartCoroutine(FadeScreen());
            if (volumeShouldFade)
                StartCoroutine(FadeOutAudio());
            // ApplyCameraEffects();

            yield return new WaitForSeconds(fadeDuration);

            yield return new WaitForSeconds(delayBeforeSceneLoad);

            DeathTransitionCompleteEvent.Trigger();

            _isProcessingDeath = false;
        }

        IEnumerator FadeScreen()
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

        IEnumerator FadeOutAudio()
        {
            if (AudioManager.Instance == null) yield break;

            var audioManager = AudioManager.Instance;
            var mmSoundManager = audioManager.SoundManager;


            var elapsed = 0f;
            while (elapsed < audioFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                var t = audioFadeCurve.Evaluate(elapsed / audioFadeOutDuration);

                mmSoundManager.SetVolumeMaster(t);
                globalAmbientAS.volume = t;
                otherEnvironmentalAS.volume = t;
                AudioListener.volume = t;

                yield return null;
            }

            mmSoundManager.SetVolumeMaster(0f);
            globalAmbientAS.volume = 0f;
            otherEnvironmentalAS.volume = 0f;
            AudioListener.volume = 0f;
        }
    }
}
