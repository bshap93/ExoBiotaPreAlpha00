using Domains.Input.Scripts;
using LevelConstruct.Interactable;
using MoreMountains.Feedbacks;
using SharedUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Interactable
{
    public class ButtonActivatedWithAction : ButtonActivatedBase
    {
        public UnityEvent OnActivation;
        public ButtonPromptWithAction ButtonPromptPrefab;

        public float scaleFactor = 1f;

        [Header("Prompt Settings")] public bool isShowing;

        public Vector3 promptTransformOffset;
        public Vector3 promptRotationOffset;

        public MMFeedbacks activationFeedback;

        [FormerlySerializedAs("PromptActionText")]
        public string PromptActionStr = "Interact";

        public Color PromptTextColor = Color.white;

        [FormerlySerializedAs("PromptKeyText")]
        public string PromptKeyStr = "E";

        ButtonPromptWithAction _buttonPrompt;

        InfoPanelActivator _infoPanelActivator;


        void Start()
        {
            if (ButtonPromptPrefab != null)
            {
                var promptPosition = transform.position + promptTransformOffset;
                var promptRotation = Quaternion.Euler(promptRotationOffset);
                _buttonPrompt = Instantiate(ButtonPromptPrefab, promptPosition, promptRotation, transform);
                _buttonPrompt.transform.localScale *= scaleFactor;
                _buttonPrompt.SetTextColor(PromptTextColor);
                _buttonPrompt.Initialization();
                _buttonPrompt.Hide();
            }

            _infoPanelActivator = GetComponent<InfoPanelActivator>();
        }

        void Update()
        {
            // if (isShowing)
            //     if (InputService.IsInteractPressed())
            //         Interact();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) ShowInteractablePrompt();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) HideInteractablePrompt();
        }


        public override void Interact()
        {
            if (_infoPanelActivator != null)
                if (_infoPanelActivator.automaticallyShowOnInteract)
                    _infoPanelActivator.ShowInfoPanel();
                else
                    _infoPanelActivator.HideInfoPanel();

            ActivateButton();
        }

        public void ShowInteractablePrompt()
        {
            if (_buttonPrompt != null)
            {
                isShowing = true;
                _buttonPrompt.Show(PromptKeyStr, PromptActionStr);
            }
        }

        public void HideInteractablePrompt()
        {
            if (_buttonPrompt != null)
            {
                isShowing = false;
                _buttonPrompt.Hide();
            }
        }

        void ActivateButton()
        {
            if (OnActivation != null)
            {
                OnActivation.Invoke();
                activationFeedback?.PlayFeedbacks();
            }
        }


        public void HidePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Hide();
        }
    }
}
