using System;
using MoreMountains.Tools;

namespace Helpers.Events.Dialog
{
    [Serializable]
    public enum SpecialDialogueEventType
    {
        RequestSpecialDialogue
    }

    public enum SpecialDialogueType
    {
        MockConsoleDataWindow
    }

    public struct SpecialDialogueEvent
    {
        static SpecialDialogueEvent _e;

        public SpecialDialogueEventType EventType;

        public string SpecialDialogueNPCID;
        public SpecialDialogueType SpecialDialogueType;

        public static void Trigger(
            SpecialDialogueEventType eventType,
            string specialDialogueNPCID,
            SpecialDialogueType specialDialogueType)
        {
            _e.EventType = eventType;
            _e.SpecialDialogueNPCID = specialDialogueNPCID;
            _e.SpecialDialogueType = specialDialogueType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
