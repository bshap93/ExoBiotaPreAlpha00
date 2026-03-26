using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using UnityEngine;

namespace NewScript.Triggere
{
    public class CreatureActivationTrigger : MonoBehaviour
    {
        [SerializeField] CreatureController[] creaturesToActivate;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
                foreach (var creature in creaturesToActivate)
                    if (creature != null)
                    {
                        creature.gameObject.SetActive(true);
                        creature.ActivateCreature();
                    }
        }


        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
                foreach (var creature in creaturesToActivate)
                    if (creature != null && creature.ShouldDeactivateUponPlayerLeavingArea)
                    {
                        creature.DeactivateCreature();
                        // creature.gameObject.SetActive(false);
                    }
        }
    }
}
