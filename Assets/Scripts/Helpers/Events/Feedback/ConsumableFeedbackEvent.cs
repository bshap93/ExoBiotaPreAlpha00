using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events.Feedback
{
    public enum ConsumableFeedbackEventType
    {
        InjectableAbilityItemUsed
    }

    public struct ConsumableFeedbackEvent
    {
        static ConsumableFeedbackEvent _e;

        public ConsumableFeedbackEventType FeedbackEventType;
        public string Id;
        public string TargetInventoryName;
        public BioticAbilityToolWrapper BioticAbilityInvItem;
        public string PlayerID;


        public static void Trigger(ConsumableFeedbackEventType feedbackEventType, string id = null)
        {
            _e.FeedbackEventType = feedbackEventType;
            _e.Id = id;

            MMEventManager.TriggerEvent(_e);
        }
        public static void Trigger(ConsumableFeedbackEventType injectableAbilityItemUsed, string bioticAbilityUniqueID,
            string targetInventoryName, BioticAbilityToolWrapper bioticAbilityInvItem, string playerID)
        {
            _e.FeedbackEventType = injectableAbilityItemUsed;
            _e.Id = bioticAbilityUniqueID;
            _e.TargetInventoryName = targetInventoryName;
            _e.BioticAbilityInvItem = bioticAbilityInvItem;
            _e.PlayerID = playerID;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
