using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FirstPersonPlayer.Combat.AINPC
{
    [Category("MoreMountains/Feedbacks")]
    public class PlayMMFeedbackOfChoice : ActionTask
    {
        public readonly BBParameter<MMFeedbacks> Feedbacks;

        protected override void OnExecute()
        {
            Feedbacks.value?.PlayFeedbacks();
            EndAction(true);
        }
    }

    [Category("MoreMountains/Feedbacks")]
    public class StopMMFeedbackOfChoice : ActionTask
    {
        public readonly BBParameter<MMFeedbacks> Feedbacks;

        protected override void OnExecute()
        {
            Feedbacks.value?.StopFeedbacks();
            EndAction(true);
        }
    }
}
