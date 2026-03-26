using MoreMountains.Tools;

namespace Helpers.Events.Dialog
{
    public enum FirstPersonDialogueEventType
    {
        StartDialogue,
        EndDialogue
    }

    public struct FirstPersonDialogueEvent
    {
        static FirstPersonDialogueEvent _e;


        public FirstPersonDialogueEventType Type;
        public string NPCId;
        public string StartNodeOverride;


        public static void Trigger(FirstPersonDialogueEventType ccet, string npcID, string startNodeOverride = null)
        {
            _e.Type = ccet;
            _e.NPCId = npcID;
            _e.StartNodeOverride = startNodeOverride;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
