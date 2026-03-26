using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Utilities.NodeCanvas.ConditionTasks
{
    [Category("BioOrganism")]
    public class IsPlayerInDetectionZone : ConditionTask<Collider>
    {
        public BBParameter<Transform> player;
    
        protected override bool OnCheck()
        {
            if (agent == null || player.value == null) return false;
            return agent.bounds.Contains(player.value.position);
        }
    }
}
