using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum InventoryEventType
    {
        ContentChanged,
        SellAllItems,
        ShowItem
    }

    /// <summary>
    ///     Secondary Inventory Event for custom events not covered by MMInventoryEvent
    /// </summary>
    public struct InventoryEvent
    {
        public static InventoryEvent E;

        public InventoryEventType EventType;

        public string InventoryId;
        public MyBaseItem Item;


        public static void Trigger(InventoryEventType eventType, string inventoryId = null, MyBaseItem item = null)
        {
            E.EventType = eventType;
            E.InventoryId = inventoryId;
            E.Item = item;


            MMEventManager.TriggerEvent(E);
        }
    }
}
