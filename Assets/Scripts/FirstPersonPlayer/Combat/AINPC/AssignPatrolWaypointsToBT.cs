using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class AssignPatrolWaypointsToBT : MonoBehaviour
    {
        public Blackboard blackboard;

        public List<GameObject> patrolWaypoints;

        public float delay = 0.5f;


        void Start()
        {
            Invoke(nameof(Assign), delay);
        }


        void Assign()
        {
            if (blackboard == null)
                blackboard = GetComponent<Blackboard>();

            blackboard.SetVariableValue("patrolWaypoints", patrolWaypoints);
            blackboard.SetVariableValue("numWaypoints", patrolWaypoints.Count);
        }
    }
}
