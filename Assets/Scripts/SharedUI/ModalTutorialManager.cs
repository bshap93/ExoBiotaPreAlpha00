using System.Collections;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalTutorialManager : MonoBehaviour
    {
        public enum CloseBehaviour
        {
            None,
            Disable,
            Destroy
        }

        public enum OnEnableBehaviour
        {
            None,
            Restore
        }

        public enum StartBehaviour
        {
            None,
            Disable,
            Enable
        }

        // Resources
        public Image windowIcon;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;
        public Image tutorialImage;
        public ButtonManager confirmButton;
        public Animator mwAnimator;

        // Content
        public Sprite icon;
        public string titleText = "Title";
        [TextArea(1, 4)] public string descriptionText = "Description here";

        // Events
        public UnityEvent onOpen = new();
        public UnityEvent onClose = new();
        [FormerlySerializedAs("onOk")] [FormerlySerializedAs("onConfirm")]
        public UnityEvent onOK = new();

        // Settings
        public bool useCustomContent;
        public bool isOn;
        public bool closeOnConfirm = true;
        public bool showConfirmButton = true;
        public StartBehaviour startBehaviour = StartBehaviour.Disable;
        public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;
        public OnEnableBehaviour onEnableBehaviour = OnEnableBehaviour.None;

        // Helpers
        float cachedStateLength;

        void Awake()
        {
            isOn = false;

            if (mwAnimator == null) mwAnimator = gameObject.GetComponent<Animator>();
            if (closeOnConfirm) onOK.AddListener(CloseWindow);
            if (confirmButton != null) confirmButton.onClick.AddListener(onOK.Invoke);
            if (startBehaviour == StartBehaviour.Disable)
            {
                isOn = false;
                gameObject.SetActive(false);
            }
            else if (startBehaviour == StartBehaviour.Enable)
            {
                isOn = false;
                OpenWindow();
            }

            cachedStateLength = MUIPInternalTools.GetAnimatorClipLength(
                mwAnimator, MUIPInternalTools.modalWindowStateName);

            UpdateUI();
        }

        void OnEnable()
        {
            if (onEnableBehaviour == OnEnableBehaviour.Restore && isOn)
            {
                isOn = false;
                Open();
            }
        }

        void OnDisable()
        {
            if (onEnableBehaviour == OnEnableBehaviour.None) isOn = false;
        }

        public void UpdateUI()
        {
            if (useCustomContent)
                return;

            if (windowIcon != null) windowIcon.sprite = icon;
            if (windowTitle != null) windowTitle.text = titleText;
            if (windowDescription != null) windowDescription.text = descriptionText;


            if (showConfirmButton && confirmButton != null)
                confirmButton.gameObject.SetActive(true);
            else if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        }

        public void Open()
        {
            if (isOn)
                return;

            isOn = true;
            gameObject.SetActive(true);
            onOpen.Invoke();

            StopCoroutine("DisableObject");
            mwAnimator.Play("Fade-in");
        }

        public void Close()
        {
            if (!isOn)
                return;

            isOn = false;
            onClose.Invoke();

            mwAnimator.Play("Fade-out");
            StartCoroutine("DisableObject");
        }

        public void AnimateWindow()
        {
            if (!isOn)
            {
                StopCoroutine("DisableObject");

                isOn = true;
                gameObject.SetActive(true);
                mwAnimator.Play("Fade-in");
            }

            else
            {
                isOn = false;
                mwAnimator.Play("Fade-out");

                StartCoroutine("DisableObject");
            }
        }

        IEnumerator DisableObject()
        {
            yield return new WaitForSecondsRealtime(cachedStateLength);

            if (closeBehaviour == CloseBehaviour.Disable)
                gameObject.SetActive(false);
            else if (closeBehaviour == CloseBehaviour.Destroy) Destroy(gameObject);
        }

        #region Obsolote

        public void OpenWindow()
        {
            Open();
        }
        public void CloseWindow()
        {
            Close();
        }

        #endregion
    }
}
