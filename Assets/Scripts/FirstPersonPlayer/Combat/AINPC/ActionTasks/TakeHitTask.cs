using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks
{
    public class TakeHitTask : ActionTask
    {
        public readonly BBParameter<AnimationClip> HitAnimationClip = null;
        public readonly BBParameter<float> HitDelay = 0.3f;
        public readonly BBParameter<MMFeedbacks> HitFeedbacks = null;

        EnemyController _enemyController;

        bool _reactionStarted;

        float _timer;

        protected override string OnInit()
        {
            _enemyController = agent.GetComponent<EnemyController>();
            return _enemyController ? null : "EnemyController component not found on the agent.";
        }

        protected override void OnExecute()
        {
            _reactionStarted = false;
            _timer = HitDelay.value;
            if (HitAnimationClip.value != null)
                _enemyController.PlayHitAnimation(HitAnimationClip.value);

            HitFeedbacks.value?.PlayFeedbacks();

            _reactionStarted = true;
        }

        protected override void OnUpdate()
        {
            if (!_reactionStarted) return;

            _timer -= Time.deltaTime;

            if (_timer <= 0f) EndAction(true);
        }

        protected override void OnStop()
        {
            HitFeedbacks.value?.StopFeedbacks();
        }
    }
}
