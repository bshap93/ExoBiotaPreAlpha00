using FirstPersonPlayer.FPNPCs.AlienNPC;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks
{
    public class NPCInitiateDialogueWithPlayer : ActionTask
    {
        bool _dialogueStarted;

        FPNPCs.AlienNPC.SlaverMotile _slaver;
        [Tooltip("Optional: override which dialogue node to start on. Leave blank to use the NPC's default.")]
        public BBParameter<string> StartNodeOverride = new("");

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            _slaver = agent.GetComponent<FPNPCs.AlienNPC.SlaverMotile>();

            if (_slaver == null)
                return $"{nameof(NPCInitiateDialogueWithPlayer)}: Agent '{agent.name}' has no SlaverMotile component.";

            return null;
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            if (!_slaver.CanInteract())
            {
                EndAction(false);
                return;
            }

            _dialogueStarted = true;

            var node = StartNodeOverride.value;

            if (string.IsNullOrWhiteSpace(node))
                _slaver.Interact();
            else
                _slaver.Interact(node);
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
            if (!_dialogueStarted)
                return;

            // The dialogue is done once the NPC leaves the InDialogue state.
            // SlaverMotile.SetState() drives CurrentState, so this is the
            // canonical signal that the conversation session has ended.
            if (_slaver.CurrentState != AlienNPCState.InDialogue)
                EndAction(true);
        }

        //Called when the task is disabled.
        protected override void OnStop()
        {
            _dialogueStarted = false;
        }

        //Called when the task is paused.
        protected override void OnPause()
        {
        }
    }
}
