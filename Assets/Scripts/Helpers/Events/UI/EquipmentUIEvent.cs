using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    public enum EquipmentUIEventType
    {
        UnequippedAbility
    }

    public struct EquipmentUIEvent
    {
        static EquipmentUIEvent _e;

        public EquipmentUIEventType Type;

        public static void Trigger(EquipmentUIEventType type)
        {
            _e.Type = type;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
