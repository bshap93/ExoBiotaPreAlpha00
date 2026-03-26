using MoreMountains.Tools;
using Structs;

namespace Helpers.Events
{
    public enum UIType
    {
        VendorConsole,
        FuelConsole,
        InfoPanel,
        CommsComputer,
        Settings,
        Any,
        InGameUI,
        AnalysisConsole,
        ModalBoxChoice,
        Dialogue,
        TutorialWindow,
        MainTutorial,
        BreakableInteractChoice,
        HarvestableInteractChoice,
        WaitWhileInteracting,
        GlobalSettingsPanel,
        MachineInteractChoice,
        RestTimeSetAmount,
        InfoLogTablet,
        LevelingUI,
        LevelingUIInfected,
        ItemInfoPopup
    }

    public enum UIActionType
    {
        Open,
        Close,
        Update
        // Toggle
    }

    public struct MyUIEvent
    {
        static MyUIEvent _e;

        public UIType uiType;
        public UIActionType uiActionType;
        public bool ShouldFreeze;

        public GameMode Index;

        public static void Trigger(UIType uiType, UIActionType uiActionType)
        {
            _e.uiType = uiType;
            _e.uiActionType = uiActionType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
