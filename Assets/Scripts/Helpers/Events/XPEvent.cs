using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum BioticCoreXPEventType
    {
        ConvertCoreToXP
    }

    public struct BioticCoreXPConversionEvent
    {
        static BioticCoreXPConversionEvent _e;
        public OuterCoreItemObject.CoreObjectValueGrade CoreGrade;
        public BioticCoreXPEventType EventType;
        public static void Trigger(BioticCoreXPEventType eventType,
            OuterCoreItemObject.CoreObjectValueGrade coreGrade)
        {
            _e.EventType = eventType;
            _e.CoreGrade = coreGrade;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
