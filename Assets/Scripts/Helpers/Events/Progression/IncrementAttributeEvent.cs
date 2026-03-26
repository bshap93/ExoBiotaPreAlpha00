using MoreMountains.Tools;
using SharedUI.Progression;

namespace Helpers.Events.Progression
{
    public struct IncrementAttributeEvent
    {
        static IncrementAttributeEvent _e;

        public AttributeType AttributeType;
        public static void Trigger(AttributeType attributeType)
        {
            _e.AttributeType = attributeType;

            MMEventManager.TriggerEvent(_e);
        }

    }
}
