using FirstPersonPlayer.Tools;
using MoreMountains.Tools;

namespace Domains.Gameplay.Mining.Events
{
    public enum ToolEventType
    {
        UseTool,
        UpgradeTool,
        ToggleToolMode
    }

    public struct ToolEvent
    {
        public static ToolEvent _e;

        public ToolEventType EventType;
        // public ToolType ToolType;
        // public ToolIteration ToolIteration;


        public static void Trigger(ToolEventType eventType
        )
        {
            _e.EventType = eventType;
            // _e.ToolType = toolType;
            // _e.ToolIteration = toolIteration;
            MMEventManager.TriggerEvent(_e);
        }
    }
}