using Helpers.Events.Gated;

namespace LevelConstruct.Interactable.ItemInteractables
{
    public interface IStatefulItemPicker
    {
        bool CanBePicked(); // ✅ Main method to decide if picking is allowed
        public int GetStateEnumIndex();
        public void SetStateEnumIndex(int index);

        public void SetStateToDefault();


        public bool IsItemPickerGated();

        public GatedInteractionType GetGatedInteractionType();

        public void PlayLoopedFeedbacks();

        public void StopLoopedFeedbacks();
    }
}
