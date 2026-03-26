using MoreMountains.Tools;

namespace Helpers.Events.Terminals
{
    public enum MetaTerminalEventType
    {
        MetaTerminalRegistered,
        RequestedFastTravelToOtherTerminal
    }

    public struct MetaTerminalEvent
    {
        static MetaTerminalEvent _e;

        public MetaTerminalEventType EventType;

        public string TerminalUniqueID;
        public string TerminalName;

        public static void Trigger(MetaTerminalEventType eventType, string terminalUniqueID, string terminalName)
        {
            _e.EventType = eventType;
            _e.TerminalUniqueID = terminalUniqueID;
            _e.TerminalName = terminalName;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
