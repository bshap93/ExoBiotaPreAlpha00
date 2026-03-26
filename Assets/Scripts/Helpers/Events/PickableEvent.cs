using MoreMountains.Tools;
using UnityEngine;

public enum PickableEventType
{
    Picked,
    // Dropped,
    PlacedItemCameToRest,
    MovedItemCameToRest
}

namespace Gameplay.Events
{
    public struct PickableEvent
    {
        static PickableEvent _e;

        // Amount is embedded in the item
        public string UniqueId;
        public PickableEventType EventType;
        public Transform ItemTransform;
        public string SOItemID;

        public static void Trigger(PickableEventType eventType, string uniqueId, Transform transform,
            string soItemID)
        {
            _e.EventType = eventType;
            _e.UniqueId = uniqueId;
            _e.ItemTransform = transform;
            _e.SOItemID = soItemID;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
