using MoreMountains.Tools;
using SharedUI.Progression;

namespace Helpers.Events.Progression
{
    public enum PendingBuyEventType
    {
        IncreasePendingAttribute,
        DecreasePendingAttribute
    }

    public struct AttrPendingBuyEvent
    {
        static AttrPendingBuyEvent _e;

        public AttributeType AttributeType;
        public PendingBuyEventType PendingBuyEventType;
        public int AttrLevelTarget;

        public static void Trigger(AttributeType attributeType, PendingBuyEventType pendingBuyEventType,
            int attrLevelTarget)
        {
            _e.AttributeType = attributeType;
            _e.PendingBuyEventType = pendingBuyEventType;
            _e.AttrLevelTarget = attrLevelTarget;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
