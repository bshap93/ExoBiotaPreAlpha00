using FirstPersonPlayer.FPNPCs.AlienNPC;
using NodeCanvas.Framework;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.Motile
{
    public class SetSlaverMotileAlienNPCState : ActionTask
    {
        public BBParameter<AlienNPCState> NewAnimancerState;
        public BBParameter<FPNPCs.AlienNPC.SlaverMotile> SlaverMotileNPCCreatureInst;
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
            SlaverMotileNPCCreatureInst.value.SetState(NewAnimancerState.value);

            EndAction(true);
        }
    }
}
