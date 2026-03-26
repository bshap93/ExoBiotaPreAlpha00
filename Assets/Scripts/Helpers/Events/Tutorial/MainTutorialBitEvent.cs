using MoreMountains.Tools;

namespace Helpers.Events.Tutorial
{
    public enum MainTutorialBitEventType
    {
        ShowMainTutBit,
        FinishTutBit,
        ShowOptionalTutorialBit,
        HideOptionalTutorialBit,
        ClearTutorialColliderTrigger
    }

    public struct MainTutorialBitEvent
    {
        static MainTutorialBitEvent _e;
        public string MainTutID;
        public string TutorialName;

        public MainTutorialBitEventType BitEventType;

        public static void Trigger(string mainTutID, MainTutorialBitEventType bitEventType, string tutorialName = null)
        {
            _e.MainTutID = mainTutID;
            _e.BitEventType = bitEventType;
            _e.TutorialName = tutorialName;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
