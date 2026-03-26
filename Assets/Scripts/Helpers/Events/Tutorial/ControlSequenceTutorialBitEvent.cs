using MoreMountains.Tools;

namespace Helpers.Events.Tutorial
{
    public struct ControlSequenceTutorialBitEvent
    {
        public enum ControlSequenceTutorialBitEventType
        {
            ShowControlPromptSequence,
            FinishControlPromptSequence
        }

        static ControlSequenceTutorialBitEvent _e;
        public string ControlPromptSequenceID;
        public ControlSequenceTutorialBitEventType BitEventType;

        public static void Trigger(string controlPromptSequenceID, ControlSequenceTutorialBitEventType bitEventType)
        {
            _e.ControlPromptSequenceID = controlPromptSequenceID;
            _e.BitEventType = bitEventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
