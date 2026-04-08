using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Interface;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public class CreatureSampleNode : BioOrganismBase, IInteractable
    {
        [SerializeField] CreatureController creatureController;
        [SerializeField] float interactionDistance = 4f;

        public void Interact()
        {
            Debug.Log("Interact");
        }
        public void Interact(string param)
        {
            // Try to switch to the Sampling tool
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            return creatureController.CurrentCreatureState != CreatureController.CreatureState.Normal;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return recognizableOnSight ? actionText : "Examine";
        }
    }
}
