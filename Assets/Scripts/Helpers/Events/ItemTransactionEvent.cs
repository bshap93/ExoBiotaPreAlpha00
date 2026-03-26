using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum ItemTransactionEventType
    {
        StartMove,
        FinishMove
    }

    public struct ItemTransactionEvent
    {
        static ItemTransactionEvent _e;

        public ItemTransactionEventType EventType;

        public static void Trigger(ItemTransactionEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
