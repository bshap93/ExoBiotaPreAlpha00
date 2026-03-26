using MoreMountains.InventoryEngine;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum ItemDiscoveryEventType
    {
        MarkKnown
    }

    public struct ItemDiscoveryEvent
    {
        private static ItemDiscoveryEvent _e;

        public string UniqueID;
        public InventoryItem Item;
        public ItemDiscoveryEventType EventType;
        public string SceneName;

        public static void Trigger(ItemDiscoveryEventType itemDiscoveryEventType, string uniqueID, InventoryItem item,
            string sceneName)
        {
            _e.EventType = itemDiscoveryEventType;
            _e.UniqueID = uniqueID;
            _e.Item = item;
            _e.SceneName = sceneName;


            MMEventManager.TriggerEvent(_e);
        }
    }
}