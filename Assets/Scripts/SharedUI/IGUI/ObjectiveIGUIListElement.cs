using Helpers.Events;
using Michsky.MUIP;
using Objectives;
using Objectives.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.IGUI
{
    public class ObjectiveIGUIListElement : MonoBehaviour
    {
        public ObjectiveObject objective;
        [SerializeField] Image objectiveImage;

        [SerializeField] TMP_Text objectiveText;

        [SerializeField] ButtonManager infoButton;
        [SerializeField] ButtonManager toggleActiveButton;

        bool _isActive;


        public void SetActive(bool active)
        {
            if (active)
            {
                ObjectiveEvent.Trigger(
                    objective.objectiveId, ObjectiveEventType.ObjectiveActivated
                );

                toggleActiveButton.SetText("Set Inactive");
            }
            else
            {
                ObjectiveEvent.Trigger(
                    objective.objectiveId, ObjectiveEventType.ObjectiveDeactivated
                );

                toggleActiveButton.SetText("Set Active");
            }

            _isActive = active;
        }

        public void GetAdditionalInfo()
        {
            // Implement the logic to show additional information about the objective, e.g., open a popup with details
            Debug.Log($"Showing additional info for objective: {objective.objectiveText}");
        }

        public void Initialize(ObjectiveObject objectiveVar)
        {
            objective = objectiveVar;
            objectiveImage.sprite = objectiveVar.objectiveImage;
            objectiveText.text = objectiveVar.objectiveText;

            infoButton.onClick.AddListener(GetAdditionalInfo);
            toggleActiveButton.onClick.AddListener(() => SetActive(!_isActive));

            if (ObjectivesManager.Instance == null)
            {
                Debug.LogError(
                    "[ObjectiveIGUIListElement] ObjectivesManager instance is null. Ensure it is initialized before using this element.");

                return;
            }

            _isActive = ObjectivesManager.Instance.IsObjectiveActive(objectiveVar.objectiveId);

            if (_isActive)
                toggleActiveButton.SetText("Set Inactive");
            else
                toggleActiveButton.SetText("Set Active");
        }
    }
}
