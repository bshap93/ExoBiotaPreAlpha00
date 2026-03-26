using Helpers.Events;
using MoreMountains.Feedbacks;

namespace LevelConstruct.Interactable.ItemInteractables
{
    public static class FirstPersonStaticDialogueHelpers
    {
        static MMFeedbacks _startDialogueFeedback;
        static int _actionId;

        public static void Initialize(MMFeedbacks startDialogueFeedback, int interactionActionId)
        {
            _startDialogueFeedback = startDialogueFeedback;
            _actionId = interactionActionId;
        }

        public static void TriggerFirstPersonDialogue()
        {
            _startDialogueFeedback?.PlayFeedbacks();
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
            ControlsHelpEvent.Trigger(
                ControlHelpEventType.Show, _actionId,
                additionalInfoText: " to Continue");
        }
    }
}
