using Animancer;
using UnityEngine;

namespace Helpers.AnimancerHelper
{
    [DisallowMultipleComponent]
    public class CamperCrabAnimationHelper : MonoBehaviour
    {
        [Header("Animancer")] [SerializeField] private AnimancerComponent animancer; // assign in inspector or auto-grab

        [Header("Clips")] [SerializeField] private AnimationClip idleClip;

        [SerializeField] private AnimationClip turnLeftClip;
        [SerializeField] private AnimationClip turnRightClip;
        [SerializeField] private AnimationClip attackClip;

        [Header("Attack")] [SerializeField] [Tooltip("Seconds between attacks (approx).")]
        private float attackCooldown = 1.5f;

        private float _nextAttackReadyTime;

        public bool IsOnAttackCooldown => Time.time < _nextAttackReadyTime;

        private void Awake()
        {
            if (!animancer) animancer = GetComponentInChildren<AnimancerComponent>();
            PlayIdle();
        }

        public void PlayIdle()
        {
            if (idleClip) animancer.Play(idleClip);
        }

        /// <summary>Triggers a short in-place turn pose. Re-trigger as needed (throttled by caller).</summary>
        public void PlayTurn(bool turnRight)
        {
            var clip = turnRight ? turnRightClip : turnLeftClip;
            if (clip) animancer.Play(clip);
        }

        /// <summary>Attempts to start an attack. Returns true if started (cooldown permits).</summary>
        public bool TryAttack()
        {
            if (attackClip == null) return false;
            if (Time.time < _nextAttackReadyTime) return false;

            animancer.Play(attackClip);

            // No events → start cooldown immediately on trigger.
            _nextAttackReadyTime = Time.time + Mathf.Max(0.05f, attackCooldown);
            return true;
        }
    }
}