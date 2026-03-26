namespace Dirigible.Interface
{
    public interface IDirigibleInteractable
    {
        void Interact();
        void OnInteractionStart();
        void OnInteractionEnd();
        bool CanInteract();

        bool IsInteractable();

        void OnFocus();

        void OnUnfocus();
        void CompleteObjectiveOnInteract();
    }
}
