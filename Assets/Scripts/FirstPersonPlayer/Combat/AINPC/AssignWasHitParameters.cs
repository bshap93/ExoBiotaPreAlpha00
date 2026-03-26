using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class AssignWasHitParameters : MonoBehaviour
    {
        public Blackboard blackboard;
        EnemyController _enemyController;

        void Start()
        {
            Invoke(nameof(Assign), 0.5f);

            _enemyController = GetComponent<EnemyController>();
        }

        void Assign()
        {
            if (blackboard == null)
                blackboard = GetComponent<Blackboard>();
            

            blackboard.SetVariableValue(
                "wasHitAnimationClip", _enemyController.creatureType.animationSet.getHitAnimation);
        }
    }
}
