using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks
{
    [Category("AttackMoves")]
    public class SingleMeleeAttack : ActionTask
    {
        // NodeCanvas Blackboard Parameters
        public readonly BBParameter<float> AttackDelay = 0.2f;
        public readonly BBParameter<int> AttackIndex = 0;
        public readonly BBParameter<float> CooldownAfterAttack = 0.5f;

        EnemyController _enemyController;
        bool _hasAttacked;
        bool _inCooldown;

        public BBParameter<MMFeedbacks> AttackFeedbacks;
        public BBParameter<float> TimeBeforeAttackFeedback = 0.0f;


        float timer;
        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            _enemyController = agent.GetComponent<EnemyController>();
            return _enemyController ? null : "EnemyController component not found on the agent.";
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            timer = AttackDelay.value;
            _hasAttacked = false;
            _inCooldown = false;
            AttackFeedbacks.value?.PlayFeedbacks();
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
            timer -= Time.deltaTime;

            // Delay phase
            if (!_hasAttacked && timer <= 0f)
            {
                StartCoroutine(_enemyController.StartAttack(AttackIndex.value));
                _hasAttacked = true;
            }

            // Wait for attack to finish
            if (_hasAttacked && !_inCooldown && !_enemyController.IsAttacking)
            {
                timer = CooldownAfterAttack.value;
                _inCooldown = true;
            }

            // Cooldown phase
            if (_inCooldown && timer <= 0f) EndAction(true);
            // Wait until attack finishes
        }

        //Called when the task is disabled.
        protected override void OnStop()
        {
        }

        //Called when the task is paused.
        protected override void OnPause()
        {
        }

        enum AttackPhase
        {
            Delay,
            Attacking,
            Cooldown
        }
    }
}
