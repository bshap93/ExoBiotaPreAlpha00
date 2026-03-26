using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    [Category("Animancer")]
    [Description("Play an Animation Clip in the stead of the default idle clip.")]
    public class CreaturePlayAnimationClip : ActionTask
    {
        public readonly BBParameter<AnimationClip> AnimationClip;
        // public readonly BBParameter<MMFeedbacks> Feedback;


        CreatureController _creatureController;
        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            _creatureController = agent.GetComponent<CreatureController>();
            return _creatureController ? null : "CreatureController component not found on the agent.";
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            // Feedback.value?.PlayFeedbacks();

            _creatureController.PlayAnimationClip(AnimationClip.value);


            EndAction(true);
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
        }

        //Called when the task is disabled.
        protected override void OnStop()
        {
        }

        //Called when the task is paused.
        protected override void OnPause()
        {
        }
    }
}
