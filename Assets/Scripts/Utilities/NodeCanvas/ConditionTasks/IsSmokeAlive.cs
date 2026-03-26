using NodeCanvas.Framework;
using OccaSoftware.ResponsiveSmokes.Runtime;
using ParadoxNotion.Design;

namespace Utilities.NodeCanvas.ConditionTasks
{
    [Category("BioOrganism")]
    public class IsSmokeAlive : ConditionTask<InteractiveSmoke>
    {
        protected override bool OnCheck()
        {
            return agent != null && agent.IsAlive();
        }
    }
}
