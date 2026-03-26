using System.Numerics;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum PickableLocationEventType
    {
        Picked,
        Dropped
    }
    public struct PickableLocationEvent
    {
        static PickableLocationEvent _e;

        public string UniqueId;

        public Vector3 Location;
    

        public static void Trigger(string uniqueId, Vector3 location)
        {
            _e.UniqueId = uniqueId;
            _e.Location = location;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
