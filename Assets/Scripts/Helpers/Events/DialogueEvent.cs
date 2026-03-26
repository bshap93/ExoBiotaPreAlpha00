using MoreMountains.Tools;

namespace Events
{
    public enum DialogueEventType
    {
        DialogueFinished,
        DialogueStarted
    }

    public struct DialogueEvent
    {
        public static DialogueEvent _e;
        public DialogueEventType EventType;

        public string NpcId;
        public string DialogueNode;

        public static void Trigger(DialogueEventType dialogueEventType, string npcId, string dialogueNode)
        {
            _e.EventType = dialogueEventType;
            _e.NpcId = npcId;
            _e.DialogueNode = dialogueNode;
            MMEventManager.TriggerEvent(_e);
        }
    }
}