using Helpers.Events.Dialog;
using UnityEngine;

public class DialogueRunnerHelper : MonoBehaviour
{
    public void TriggerDialogueEndEvent(string npcID)
    {
        FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.EndDialogue, npcID);
    }
}
