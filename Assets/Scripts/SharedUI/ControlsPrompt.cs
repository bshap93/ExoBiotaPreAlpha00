using System.Collections;
using DG.Tweening;
using Helpers.Events;
using Helpers.Events.Tutorial;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI
{
    public class ControlsPrompt : MonoBehaviour, MMEventListener<ControlsHelpEvent>,
        MMEventListener<MainTutorialBitEvent>, MMEventListener<SpontaneousTriggerEvent>
    {
        [SerializeField] ControlsPromptSchemeSet defaultKeyboardSchemeSet;

        [SerializeField] string uniqueId;

        [SerializeField] Image promptImage;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text additionInfoText;

        [SerializeField] MMFeedbacks useFeedbacks;

        [SerializeField] TMP_Text with;
        [SerializeField] Image toolIcon;

        [SerializeField] float useFBDuration = 1f;
        [SerializeField] float promptTimeoutDuration = 2.5f;
        bool _blockCloseRequests;
        bool _blockNewOpenRequests;

        CanvasGroup _canvasRenderer;

        float _currentPromptTimeout;
        bool _isShowingAControlsPrompt;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _canvasRenderer = GetComponent<CanvasGroup>();
            _canvasRenderer.alpha = 0;
        }

        void Update()
        {
            if (_isShowingAControlsPrompt)
            {
                _currentPromptTimeout += Time.deltaTime;
                if (_currentPromptTimeout >= promptTimeoutDuration)
                {
                    _currentPromptTimeout = 0;
                    UnsetPromptImage();
                    Hide();
                }
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<ControlsHelpEvent>();
            this.MMEventStartListening<MainTutorialBitEvent>();
            this.MMEventStartListening<SpontaneousTriggerEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<ControlsHelpEvent>();
            this.MMEventStopListening<MainTutorialBitEvent>();
            this.MMEventStopListening<SpontaneousTriggerEvent>();
        }

        public void OnMMEvent(ControlsHelpEvent eventType)
        {
            if (eventType.EventType == ControlHelpEventType.ShowIfNothingElseShowing)
            {
                // if (_blockNewOpenRequests) return;
                if (_isShowingAControlsPrompt) return;

                ShowAppropriatePrompt(eventType);
                return;
            }

            if (eventType.EventType == ControlHelpEventType.Show)
            {
                ShowAppropriatePrompt(eventType);
            }
            else if (eventType.EventType == ControlHelpEventType.Hide)
            {
                if (!_blockCloseRequests)
                {
                    UnsetPromptImage();
                    additionInfoText.text = "";
                    with.text = "";
                    toolIcon.enabled = false;
                    Hide();
                }
            }
            else if (eventType.EventType == ControlHelpEventType.ShowUseThenHide)
            {
                if (!_blockNewOpenRequests)
                    StartCoroutine(ShowUseThenHide());
            }
            else if (eventType.EventType == ControlHelpEventType.ShowThenHide)
            {
                if (!_blockNewOpenRequests)
                    StartCoroutine(ShowThenHide());
            }

            if (!string.IsNullOrEmpty(eventType.AdditionalInstruction))
            {
                if (eventType.AdditionalInstruction == "BlockAllNewRequests")
                {
                    _blockCloseRequests = true;
                    _blockNewOpenRequests = true;
                }
                else if (eventType.AdditionalInstruction == "UnblockAllRequests")
                {
                    _blockCloseRequests = false;
                    _blockNewOpenRequests = false;
                }
            }
        }
        public void OnMMEvent(MainTutorialBitEvent bitEventType)
        {
            if (bitEventType.BitEventType == MainTutorialBitEventType.ShowMainTutBit) Hide();
        }
        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
            if (eventType.IntParameter == -1) return;
            if (eventType.UniqueID == uniqueId)
            {
                if (eventType.StringParameter != null) additionInfoText.text = eventType.StringParameter;

                if (eventType.EventType == SpontaneousTriggerEventType.Triggered)
                    Show();
            }
        }
        void ShowAppropriatePrompt(ControlsHelpEvent eventType)
        {
            if (!_blockNewOpenRequests)
            {
                var datum = defaultKeyboardSchemeSet.GetDatumByActionId(eventType.ActionId);

                if (string.IsNullOrEmpty(eventType.AdditionalInfoText))
                    additionInfoText.text = eventType.AdditionalInfoText;

                if (datum.PromptIcon != null && datum.PromptText != null &&
                    eventType.ToolIcon != null)
                {
                    SetPromptElements(datum.PromptIcon, datum.PromptText);
                    // additionInfoText.text = eventType.AdditionalInstruction;
                    with.text = "with";
                    toolIcon.enabled = true;
                    toolIcon.sprite = eventType.ToolIcon;
                    Show();
                }
                else if (datum.PromptIcon != null && datum.PromptText != null &&
                         !string.IsNullOrEmpty(eventType.AdditionalInfoText)
                        )
                {
                    SetPromptElements(datum.PromptIcon, datum.PromptText);
                    additionInfoText.text = eventType.AdditionalInfoText;
                    with.text = "";
                    toolIcon.enabled = false;
                    Show();
                }
                else if (datum.PromptIcon != null && datum.PromptText != null && datum.AdditionalContext != null)
                {
                    SetPromptElements(datum.PromptIcon, datum.PromptText);
                    additionInfoText.text = datum.AdditionalContext;
                    with.text = "";
                    toolIcon.enabled = false;
                    Show();
                }


                else if (datum.PromptIcon != null && datum.PromptText != null)
                {
                    SetPromptElements(datum.PromptIcon, datum.PromptText);
                    with.text = "";
                    toolIcon.enabled = false;
                    Show();
                }
                else if (datum.PromptText != null)
                {
                    SetPromptTextOnly(datum.PromptText);
                    with.text = "";
                    toolIcon.enabled = false;
                    Show();
                }
                else if (datum.PromptIcon != null)
                {
                    SetPromptImageOnly(datum.PromptIcon);
                    with.text = "";
                    toolIcon.enabled = false;
                    Show();
                }
            }
        }

        IEnumerator ShowUseThenHide()
        {
            useFeedbacks?.PlayFeedbacks();
            // Wait n seconds
            yield return new WaitForSeconds(useFBDuration);
            UnsetPromptImage();
            Hide();
        }

        IEnumerator ShowThenHide(float duration = 1f)
        {
            Show();
            // Wait n seconds
            yield return new WaitForSeconds(duration);
            UnsetPromptImage();
            Hide();
        }

        public void SetPromptTextOnly(string textContent)
        {
            promptImage.sprite = null;
            promptImage.enabled = false;
            promptText.text = textContent;
        }

        public void SetPromptImageOnly(Sprite newSprite)
        {
            promptImage.enabled = true;
            promptImage.sprite = newSprite;
            promptText.text = "";
        }

        public void SetPromptElements(Sprite newSprite, string textContent)
        {
            promptImage.enabled = true;
            promptImage.sprite = newSprite;
            promptText.text = textContent;
        }

        public void Show()
        {
            _canvasRenderer.alpha = 1;
            _isShowingAControlsPrompt = true;
        }

        public void UnsetPromptImage()
        {
            promptImage.sprite = null;
            promptImage.enabled = false;

            promptText.text = "";
            additionInfoText.text = "";
            with.text = "";
            toolIcon.enabled = false;
        }

        public void Hide()
        {
            _canvasRenderer.DOFade(0, 0.25f);
            _isShowingAControlsPrompt = false;
        }
    }
}
