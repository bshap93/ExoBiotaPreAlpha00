namespace FirstPersonPlayer.Interface
{
    public interface IInteractable
    {
        void Interact();
        void Interact(string param);
        void OnInteractionStart();
        void OnInteractionEnd(string param);
        bool CanInteract();

        bool IsInteractable();

        void OnFocus();

        void OnUnfocus();

        float GetInteractionDistance();
    }
}
