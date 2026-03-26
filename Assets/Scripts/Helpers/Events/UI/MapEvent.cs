using MoreMountains.Tools;
using SharedUI.Interact.Map;
using UnityEngine;

namespace Helpers.Events.UI
{
    public struct MapEvent
    {
        static MapEvent _e;

        public enum MapEventType
        {
            OpenedMap,
            ClosedMap
        }
        
        public Vector2 PlayerMapPosition;
        public MapObject MapObject;

        public MapEventType EventType;

        public static void Trigger(MapEventType eventType)
        {
            _e.EventType = eventType;

            MMEventManager.TriggerEvent(_e);
        }
        
    }
}
