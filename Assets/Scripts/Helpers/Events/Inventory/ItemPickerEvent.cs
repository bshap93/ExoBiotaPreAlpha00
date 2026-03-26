using MoreMountains.Tools;

namespace Helpers.Events.Inventory
{
    public struct ItemPickerEvent
    {
        static ItemPickerEvent _e;

        public enum ItemPickerEventType
        {
            TriggerHighlight
        }

        public ItemPickerEventType EventType;
        public string ItemPickerUniqueID;

        public static void Trigger(ItemPickerEventType eventType, string itemPickerUniqueID)
        {
            _e.EventType = eventType;
            _e.ItemPickerUniqueID = itemPickerUniqueID;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
