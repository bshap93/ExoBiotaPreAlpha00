using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Interface;
using Helpers.Events.Creature;
using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public class CreatureSampleNode : BioOrganismBase, IInteractable, MMEventListener<CreatureStateChangeEvent>
    {
        [SerializeField] CreatureController creatureController;
        [SerializeField] float interactionDistance = 4f;

        bool _isDead;

        protected override void OnEnable()
        {
            this.MMEventStartListening();
        }

        protected override void OnDisable()
        {
            this.MMEventStopListening();
        }

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
        public void OnMMEvent(CreatureStateChangeEvent eventType)
        {
            if (creatureController.uniqueID == eventType.CreatureUniqueId)
                if (eventType.NewState == CreatureController.CreatureState.Dead)
                    _isDead = true;
        }
        public override string GetName()
        {
            if (_isDead) return "Dead " + bioOrganismType.organismName;
            return bioOrganismType != null ? bioOrganismType.organismName : "Unknown Organism";
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return recognizableOnSight ? actionText : "Examine";
        }
    }
}
