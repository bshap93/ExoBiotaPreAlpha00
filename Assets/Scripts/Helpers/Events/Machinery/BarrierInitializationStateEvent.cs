using Manager.StateManager;
using MoreMountains.Tools;

namespace Helpers.Events.Machinery
{
    public enum BarrierStateEventType
    {
        SetNewBarrierState
    }

    public struct BarrierInitializationStateEvent
    {
        static BarrierInitializationStateEvent _e;

        public string UniqueID;
        public BarrierStateEventType EventType;
        public ConditionalBarrierManager.BarrierInitializationState BarrierInitializationState;


        public static void Trigger(BarrierStateEventType eventType, string uniqueID,
            ConditionalBarrierManager.BarrierInitializationState barrierInitializationState)
        {
            _e.EventType = eventType;
            _e.UniqueID = uniqueID;
            _e.BarrierInitializationState = barrierInitializationState;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
