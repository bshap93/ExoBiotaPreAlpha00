using Objectives;
using Objectives.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.IGUI
{
    public class ObjectiveHUDListElement : MonoBehaviour
    {
        public ObjectiveObject objective;
        [SerializeField] Image objectiveImage;

        [SerializeField] TMP_Text objectiveText;
        [SerializeField] TMP_Text numberText;


        bool _isActive;


        public void Initialize(ObjectiveObject objectiveVar, int objectiveProgress)
        {
            objective = objectiveVar;
            objectiveImage.sprite = objectiveVar.objectiveImage;
            objectiveText.text = objectiveVar.objectiveText;
            if (objectiveVar.objectiveProgressType == ObjectiveProgressType.DoThingNTimes)
            {
                numberText.enabled = true;
                numberText.text = $"{objectiveProgress}/{objectiveVar.targetProgress}";
            }
            else
            {
                numberText.enabled = false;
            }


            if (ObjectivesManager.Instance == null)
                Debug.LogError(
                    "[ObjectiveIGUIListElement] ObjectivesManager instance is null. Ensure it is initialized before using this element.");
        }

        public void UpdateProgress(int newProgress)
        {
            if (objective.objectiveProgressType != ObjectiveProgressType.DoThingNTimes) return;
            numberText.text = $"{newProgress}/{objective.targetProgress}";
        }
    }
}
