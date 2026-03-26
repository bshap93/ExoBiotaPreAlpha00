using MoreMountains.Tools;

namespace Helpers.Events.Inventory
{
    public enum ItemInfoUIEventType
    {
        ShowNewItemType
    }

    public struct ItemInfoUIEvent
    {
        static ItemInfoUIEvent _e;

        public ItemInfoUIEventType EventType;
        public string ItemId;

        public static void Trigger(ItemInfoUIEventType eventType, string itemId)
        {
            _e.EventType = eventType;
            _e.ItemId = itemId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
