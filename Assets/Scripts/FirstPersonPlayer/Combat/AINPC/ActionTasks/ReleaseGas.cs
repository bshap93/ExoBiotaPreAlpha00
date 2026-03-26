using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks
{
    [Category("AttackMoves")]
    public class ReleaseGas : ActionTask
    {
        public readonly BBParameter<float> CooldownAfterGasPuff = 8f;

        public readonly BBParameter<float> GasPuffDelay = 0.2f;

        public readonly BBParameter<MMFeedbacks> PuffGasFeedbacks;
        CessileGasCreatureController _creatureController;
        bool _hasPuffedGas;
        bool _inCooldown;

        float timer;
        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            _creatureController = agent.GetComponent<CessileGasCreatureController>();
            return _creatureController ? null : "CessileGasCreatureController component not found on the agent.";
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            timer = GasPuffDelay.value;
            _hasPuffedGas = false;
            _inCooldown = false;
            // PuffGasFeedbacks.value?.PlayFeedbacks();
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
            timer -= Time.deltaTime;

            // Delay phase
            if (!_hasPuffedGas && timer <= 0f)
            {
                _creatureController.StartPuffGas();
                _hasPuffedGas = true;
            }

            if (_hasPuffedGas && !_inCooldown && !_creatureController.IsPuffingGas)
            {
                timer = CooldownAfterGasPuff.value;
                _inCooldown = true;
            }

            if (_inCooldown && timer <= 0f) EndAction(true);
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
