using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.SlaverMotile
{
    [Category("SlaverMotile")]
    public class SetSlaverFlagTask : ActionTask
    {
        public readonly BBParameter<bool> FlagValue = new();
        public readonly BBParameter<FPNPCs.AlienNPC.SlaverMotile.SlaverFlagType> SlaverFlagType = new();

        FPNPCs.AlienNPC.SlaverMotile _slaverMotile;

        protected override string OnInit()
        {
            _slaverMotile = agent.GetComponent<FPNPCs.AlienNPC.SlaverMotile>();
            return _slaverMotile ? null : "SlaverMotile component not found on the agent.";
        }

        protected override void OnExecute()
        {
            _slaverMotile.SetSlaverFlag(SlaverFlagType.value, FlagValue.value);
            EndAction(true);
        }
    }
}
