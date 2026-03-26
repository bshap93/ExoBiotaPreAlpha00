using MoreMountains.Tools;

namespace Helpers.Events.Terminals
{
    public enum ElevatorRootSystemEventType
    {
        /// <summary>Player has requested the elevator moves to a destination.</summary>
        OrderToDestination,

        /// <summary>Elevator has finished travelling and locked onto a destination.</summary>
        ArrivedAtDestination
    }

    public struct ElevatorRootSystemEvent
    {
        static ElevatorRootSystemEvent _e;

        public ElevatorRootSystemEventType EventType;
        public string ElevatorSystemId;
        public string DestinationId;

        /// <summary>Fire an <see cref="ElevatorRootSystemEventType.OrderToDestination" /> event.</summary>
        public static void Trigger(string elevatorSystemId, string destinationId)
        {
            Trigger(ElevatorRootSystemEventType.OrderToDestination, elevatorSystemId, destinationId);
        }

        /// <summary>Fire any elevator event type.</summary>
        public static void Trigger(ElevatorRootSystemEventType type, string elevatorSystemId, string destinationId)
        {
            _e.EventType = type;
            _e.ElevatorSystemId = elevatorSystemId;
            _e.DestinationId = destinationId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
