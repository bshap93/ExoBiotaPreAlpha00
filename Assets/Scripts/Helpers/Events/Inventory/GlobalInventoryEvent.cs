using System;
using MoreMountains.Tools;

namespace Helpers.Events.Inventory
{
    [Serializable]
    public enum GlobalInventoryEventType
    {
        UnequipRightHandTool
    }

    public struct GlobalInventoryEvent
    {
        static GlobalInventoryEvent _e;

        public GlobalInventoryEventType EventType;

        public static void Trigger(GlobalInventoryEventType unequipRightHandTool)
        {
            _e.EventType = unequipRightHandTool;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
