using Helpers.Events.Tutorial;
using Helpers.ScriptableObjects.Tutorial;
using TMPro;
using UnityEngine;

public class TutorialElementStatic : MonoBehaviour
{
    public MainTutBitWindowArgs tutBit;
    [SerializeField] TMP_Text textField;

    void OnEnable()
    {
        textField.text = tutBit.tutBitName;
    }

    public void TriggerEvent()
    {
        MainTutorialBitEvent.Trigger(tutBit.mainTutID, MainTutorialBitEventType.ShowMainTutBit);
    }
}
