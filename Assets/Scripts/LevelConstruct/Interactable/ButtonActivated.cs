using Domains.Input.Scripts;
using LevelConstruct.Interactable;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Interactable
{
    public class ButtonActivated : ButtonActivatedBase
    {
        public UnityEvent OnActivation;
        public ButtonPrompt ButtonPromptPrefab;

        public Vector3 promptTransformOffset;
        public Vector3 promptRotationOffset;

        public MMFeedbacks activationFeedback;

        public float scaleFactor = 1f;

        public Color PromptTextColor = Color.white;

        public float ShowPromptDelay;

        [FormerlySerializedAs("PromptKeyText")]
        public string PromptKeyStr = "E";

        ButtonPrompt _buttonPrompt;

        void Start()
        {
            if (ButtonPromptPrefab != null)
            {
                var promptPosition = transform.position + promptTransformOffset;
                var promptRotation = Quaternion.Euler(promptRotationOffset);
                _buttonPrompt = Instantiate(ButtonPromptPrefab, promptPosition, promptRotation, transform);
                _buttonPrompt.transform.localScale *= scaleFactor;
                _buttonPrompt.Initialization();
                _buttonPrompt.Hide();
            }
        }

        public override void Interact()
        {
            ActivateButton();
        }

        public void ShowInteractablePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Show(PromptKeyStr);
        }

        public void HideInteractablePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Hide();
        }

        void ActivateButton()
        {
            if (OnActivation != null)
            {
                OnActivation.Invoke();
                activationFeedback?.PlayFeedbacks();
            }
        }
    }
}
