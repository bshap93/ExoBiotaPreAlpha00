using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events
{
    public enum ControlHelpEventType
    {
        Show,
        Hide,
        ShowUseThenHide,
        ShowThenHide,
        ShowIfNothingElseShowing
    }

    public struct ControlsHelpEvent
    {
        static ControlsHelpEvent _e;
        public int ActionId;
        public ControlHelpEventType EventType;
        public string AdditionalInstruction;
        public string AdditionalInfoText;
        public Sprite ToolIcon;

        public static void
            Trigger(ControlHelpEventType eventType, int actionId,
                string additionalInstruction = null, Sprite toolIcon = null, string additionalInfoText = null)
        {
            _e.EventType = eventType;
            _e.ActionId = actionId;
            _e.AdditionalInstruction = additionalInstruction;
            _e.AdditionalInfoText = additionalInfoText;
            _e.ToolIcon = toolIcon;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
