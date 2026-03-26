using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class CessileCreatureBBSync : MonoBehaviour
    {
        Blackboard _bb;
        CessileGasCreatureController _creatureController;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _creatureController = GetComponent<CessileGasCreatureController>();
            _bb = GetComponent<Blackboard>();
        }

        void Start()
        {
            _bb.SetVariableValue("maxHealth", _creatureController.MaxHealth);
            _bb.SetVariableValue("detectionRadius", _creatureController.detectionRadius);
            _bb.SetVariableValue("stunDamageThreshold", _creatureController.StunThreshold);
        }

        // Update is called once per frame
        void Update()
        {
            _bb.SetVariableValue("currentHealth", _creatureController.currentHealth);
            _bb.SetVariableValue("stunDamage", _creatureController.currentStunDamage);
            _bb.SetVariableValue("isPuffingGas", _creatureController.IsPuffingGas);

            _bb.SetVariableValue("isDead", _creatureController.isDead);
            _bb.SetVariableValue("hazardActive", _creatureController.HazardActive);
            _bb.SetVariableValue("isStunned", _creatureController.isStunned);
        }
    }
}
