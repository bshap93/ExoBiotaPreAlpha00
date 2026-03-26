using MoreMountains.Tools;

namespace Helpers.Events.Machinery
{
    public enum ElevatorEventType
    {
        SetNextFloor
    }

    public struct ElevatorEvent
    {
        static ElevatorEvent _e;

        public string ElevatorUniqueID;
        public ElevatorEventType ElevatorEventType;
        public int TargetFloor;

        public static void Trigger(string elevatorUniqueID, ElevatorEventType elevatorEventType, int targetFloor = -1)
        {
            _e.ElevatorUniqueID = elevatorUniqueID;
            _e.ElevatorEventType = elevatorEventType;
            _e.TargetFloor = targetFloor;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
