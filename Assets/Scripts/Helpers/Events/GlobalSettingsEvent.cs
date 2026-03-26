using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum GlobalSettingsEventType
    {
        ResolutionChanged,
        DitheringToggled,
        MouseXSensitivityChanged,
        MouseYSensitivityChanged,
        TutorialOnChanged,
        AutoSaveAtCheckpointsChanged,
        FullScreenModeChanged,
        FieldOfViewChanged
    }

    public enum ValueType
    {
        ChoiceIndex,
        FloatValue
    }

    public struct GlobalSettingsEvent
    {
        static GlobalSettingsEvent _e;
        public GlobalSettingsEventType EventType;
        public int ChoiceIndex;
        public float FloatValue;

        // trigger
        public static void Trigger(GlobalSettingsEventType eventType, int resolutionIndex)
        {
            _e.EventType = eventType;
            _e.ChoiceIndex = resolutionIndex;
            MMEventManager.TriggerEvent(_e);
        }

        public static void Trigger(GlobalSettingsEventType eventType, float floatValue)
        {
            _e.EventType = eventType;
            _e.ChoiceIndex = -1;
            _e.FloatValue = floatValue;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
