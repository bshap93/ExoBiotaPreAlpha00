using FirstPersonPlayer.Interactable.Stateful;
using MoreMountains.Tools;

namespace Helpers.Events.Machine
{
    public enum ElevatorStateEventType
    {
        SetNewFloorState
    }

    public struct ElevatorStateEvent
    {
        static ElevatorStateEvent _e;

        public string UniqueID;
        public StatefulElevator.ElevatorState State;
        public ElevatorStateEventType EventType;

        public static void Trigger(string uniqueID, StatefulElevator.ElevatorState state,
            ElevatorStateEventType eventType)
        {
            _e.UniqueID = uniqueID;
            _e.State = state;
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
