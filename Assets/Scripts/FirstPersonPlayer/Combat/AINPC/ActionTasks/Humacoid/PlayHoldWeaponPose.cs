using FirstPersonPlayer.FPNPCs.AlienNPC;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.Humacoid
{
    public class PlayHoldWeaponPose : ActionTask
    {
        public BBParameter<AnimationClip> EquippedHoldPose;
        public BBParameter<AlienNPCAnimancerController> HumanoidNPCCreatureInst;

        protected override void OnExecute()
        {
            if (HumanoidNPCCreatureInst.value != null)
                HumanoidNPCCreatureInst.value.PlayWeaponHoldPose(EquippedHoldPose.value);

            EndAction(true);
        }
    }
}
