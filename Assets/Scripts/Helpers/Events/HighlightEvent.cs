using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum HighlightableType
    {
        RhizomicCores,
        LiquidSampleables
    }

    public struct HighlightEvent
    {
        static HighlightEvent _e;

        public HighlightableType HighlightableType;
        public bool State;


        public static void Trigger(HighlightableType highlightableType, bool state)
        {
            _e.HighlightableType = highlightableType;
            _e.State = state;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
