using MoreMountains.Tools;
using SharedUI.Progression;

namespace Helpers.Events
{
    public struct AttributeLevelUpEvent
    {
        static AttributeLevelUpEvent _e;
        public AttributeType AttributeType;
        public int NewLevel;
        public static void Trigger(AttributeType attributeType, int newLevel)
        {
            _e.AttributeType = attributeType;
            _e.NewLevel = newLevel;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
