using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum ShoppingEventType
    {
        StartShoppingBuy,
        StartShoppingSell,
        StopShoppingBuy,
        StopShoppingSell,
        SoldItem,
        BoughtItem,
        StartShoppingSellIllegal
    }

    public struct ShoppingEvent
    {
        static ShoppingEvent _e;

        public string NpcId;
        public ShoppingEventType EventType;
        public int CurrentQuantity;
        public MyBaseItem CurrentItem;
        public string InventoryId;

        public static void Trigger(string npcId, ShoppingEventType eventType, int currentQuantity = 1,
            MyBaseItem currentItem = null, string inventoryId = "PlayerMainInventory")
        {
            _e.NpcId = npcId;
            _e.CurrentQuantity = currentQuantity;
            _e.CurrentItem = currentItem;
            _e.EventType = eventType;
            _e.InventoryId = inventoryId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
