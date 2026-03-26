using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    /// <summary>
    ///     Hotbar event structure for communication between hotbar systems
    /// </summary>
    public struct HotbarEvent
    {
        public enum HotbarEventType
        {
            AddToHotbar,
            RemoveFromHotbar,
            ConsumableHotbarChanged,
            ToolHotbarChanged,
            SelectConsumableSlot,
            SelectToolSlot,
            RefreshAllHotbars,
            HideHotbars,
            ShowHotbars
        }

        public HotbarEventType EventType;
        public string ItemID;
        public int IndexInInventory;
        public int SlotIndex;


        static HotbarEvent _e;

        public static void Trigger(HotbarEventType eventType, string itemID = "Any", int indexOrSlot = -1)
        {
            _e.EventType = eventType;
            _e.ItemID = itemID;
            _e.IndexInInventory = indexOrSlot;
            _e.SlotIndex = indexOrSlot;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
