using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events
{
    namespace Domains.Player.Events
    {
        public enum DestructableEventType
        {
            Destroyed
        }

        public struct DestructableEvent
        {
            static DestructableEvent _e;
            public string UniqueID;
            public Transform ItemTransform; // NEW: lets the manager infer the scene

            public DestructableEventType EventType;

            // New preferred overload (with Transform)
            public static void Trigger(DestructableEventType eventType, string uniqueID, Transform itemTransform)
            {
                _e.EventType = eventType;
                _e.UniqueID = uniqueID;
                _e.ItemTransform = itemTransform;
                MMEventManager.TriggerEvent(_e);
            }

            // Back-compat overload (no Transform)
            public static void Trigger(DestructableEventType eventType, string uniqueID)
            {
                Trigger(eventType, uniqueID, null);
            }
        }
    }
}
