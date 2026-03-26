using Objectives.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Objectives.UI
{
    public class ObjectiveDisplayItem : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Status;

        public void Display(ObjectiveObject obj, string status)
        {
            if (Icon) Icon.sprite = obj.objectiveImage;
            if (Title) Title.text = string.IsNullOrEmpty(obj.objectiveText) ? obj.objectiveId : obj.objectiveText;
            if (Status) Status.text = status;
        }

        public void SetStatus(string status)
        {
            if (Status) Status.text = status;
        }
    }
}