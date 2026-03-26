using Helpers.Events.Status;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

// Your PlayerStatsEvent

namespace Utilities.NodeCanvas.ActionTasks
{
    [Category("BioOrganism")]
    public class ContaminateWhileInside : ActionTask<Collider>
    {
        public float contaminationPerSecond = 2f;
        public BBParameter<Transform> player;

        protected override void OnUpdate()
        {
            if (agent && player.value && agent.bounds.Contains(player.value.position))
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    contaminationPerSecond * Time.deltaTime);
        }
    }
}
