using System;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Interface;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public class CreatureSampleNode : BioOrganismBase, IInteractable
    {
        [SerializeField] CreatureController creatureController;

        public void Interact()
        {
            Debug.Log("Interact");
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return recognizableOnSight ? actionText : "Examine";
        }
    }
}
