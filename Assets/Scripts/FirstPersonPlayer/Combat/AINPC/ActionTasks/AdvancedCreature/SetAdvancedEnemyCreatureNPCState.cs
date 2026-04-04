using FirstPersonPlayer.FPNPCs.AlienNPC;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using NodeCanvas.Framework;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.AdvancedCreature
{
    public class SetAdvancedEnemyCreatureNPCState : ActionTask
    {
        public BBParameter<AdvancedCreatureEnemyController> CreatureController;
        public BBParameter<bool> IsHostile;
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
            if (CreatureController.value != null)
                CreatureController.value.SetState(NewState.value, IsHostile.value);

            EndAction(true);
        }
    }
}
