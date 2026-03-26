using FirstPersonPlayer.FPNPCs.AlienNPC;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using NodeCanvas.Framework;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.Humacoid
{
    public class SetNonHumanoidAlienNPCState : ActionTask
    {
        public BBParameter<HumanoidNPCCreature> HumanoidNPCCreature;
        public BBParameter<AlienNPCState> NewState;
        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            return null;
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            EndAction(true);
        }
    }
}
