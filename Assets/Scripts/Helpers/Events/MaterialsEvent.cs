using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum MaterialEventType
    {
        DissolvableReset
    }

    public struct MaterialsEvent
    {
        static MaterialsEvent _e;
        public MaterialEventType EventType;
        public string MaterialName;

        public static void Trigger(MaterialEventType eventType, string materialName)
        {
            _e.EventType = eventType;
            _e.MaterialName = materialName;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
