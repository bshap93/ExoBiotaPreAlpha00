using System.Collections;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.AINPC;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using Helpers.Events.NPCs;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace FirstPersonPlayer.Interactable.BioOrganism.Creatures
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [DisallowMultipleComponent]
    public class EnemyController : CreatureController
    {
        [Header("Movement & Pathfinding")] [SerializeField]
        protected NavMeshAgent navMeshAgent;
        [SerializeField] protected float movementSpeedThreshold = 0.1f;
        [SerializeField] protected float walkRunThreshold = 2f;


        [Header("Ranged Attack")] [SerializeField]
        bool hasRangedAttack;
        [ShowIf("hasRangedAttack")] [SerializeField]
        Transform rangedAttackOrigin;


        [Header("Layers")] [SerializeField] int onDeathLayer;

        [Header("Flags")] [SerializeField] protected bool doNotUseIdleState;


        [Header("Enemy Controller Feedbacks")] [SerializeField]
        protected MMFeedbacks movementLoopFeedbacks;
        [SerializeField] GameObject deathParticlesDustPrefab;

        Tween _hitTween;
        AnimancerState _runState;
        AnimancerState _strafeLeftState;
        AnimancerState _strafeRightState;
        AnimancerState _walkBackState;

        protected AnimancerState _walkState;

        protected AnimancerState AttackState;
        protected AnimancerState DeathState;

        public bool HasHitPlayerThisAttack { get; private set; }


        public bool IsAttacking { get; set; }

        protected override void Awake()
        {
            if (startAsActivated)
                SetupAnimationStates();
            else DeactivateCreature();
        }


        protected virtual void Update()
        {
            if (!IsActivated) return;
            if (IsAttacking) return; // Only attacks block everything

            var speed = navMeshAgent.velocity.magnitude;
            var velocity = navMeshAgent.velocity;

            if (speed < movementSpeedThreshold)
            {
                if (!doNotUseIdleState)
                    // Idle should NOT interrupt custom animations
                    if (!IsPlayingCustomAnimation && !IdleState.IsPlaying)
                        animancerComponent.Play(IdleState, 0.2f);

                movementLoopFeedbacks?.StopFeedbacks();
            }
            else
            {
                PlayMovementAnimation(velocity, speed);
                IsPlayingCustomAnimation = false;

                if (movementLoopFeedbacks != null && !movementLoopFeedbacks.IsPlaying)
                    movementLoopFeedbacks.PlayFeedbacks();
            }


            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                DeathState = animancerComponent.Play(creatureType.animationSet.deathAnimation, 0.1f);
                DeathState.Events(this).OnEnd = () =>
                {
                    if (destroyAfterDeath)
                        Destroy(gameObject);
                };

                OnDeath();
            }
        }

        protected virtual void PlayMovementAnimation(Vector3 worldVelocity, float speed)
        {
            var animSet = creatureType.animationSet;

            // Non-humanoid: single movement clip, same as before.
            if (!animSet.IsHumanoid)
            {
                if (!MoveState.IsPlaying)
                    animancerComponent.Play(MoveState, 0.2f);

                return;
            }

            // Project velocity onto local axes to determine direction.
            var localVelocity = transform.InverseTransformDirection(worldVelocity);
            var forwardSpeed = localVelocity.z; // positive = forward, negative = backward
            var lateralSpeed = localVelocity.x; // positive = right, negative = left

            var isMovingBackward = forwardSpeed < -0.3f;
            var isStrafeRight = !isMovingBackward && lateralSpeed > 0.3f;
            var isStrafeLeft = !isMovingBackward && lateralSpeed < -0.3f;

            if (isMovingBackward && animSet.walkBackAnimation != null)
            {
                _walkBackState ??= animancerComponent.States.GetOrCreate(animSet.walkBackAnimation);
                _walkBackState.Speed = animSet.walkBackAnimationSpeedMultiplier;
                if (!_walkBackState.IsPlaying) animancerComponent.Play(_walkBackState, 0.2f);
            }
            else if (isStrafeLeft && animSet.strafeLeftAnimation != null)
            {
                _strafeLeftState ??= animancerComponent.States.GetOrCreate(animSet.strafeLeftAnimation);
                _strafeLeftState.Speed = animSet.strafeAnimationSpeedMultiplier;
                if (!_strafeLeftState.IsPlaying) animancerComponent.Play(_strafeLeftState, 0.2f);
            }
            else if (isStrafeRight && animSet.strafeRightAnimation != null)
            {
                _strafeRightState ??= animancerComponent.States.GetOrCreate(animSet.strafeRightAnimation);
                _strafeRightState.Speed = animSet.strafeAnimationSpeedMultiplier;
                if (!_strafeRightState.IsPlaying) animancerComponent.Play(_strafeRightState, 0.2f);
            }
            else
            {
                // Forward movement: walk vs. run.
                if (speed >= walkRunThreshold && animSet.runActionClip != null)
                {
                    _runState ??= animancerComponent.States.GetOrCreate(animSet.runActionClip.animationClip);
                    _runState.Speed = animSet.runAnimationSpeedMultiplier;
                    if (!_runState.IsPlaying) animancerComponent.Play(_runState, 0.2f);
                }
                else
                {
                    var walkClip = animSet.walkActionClip != null
                        ? animSet.walkActionClip.animationClip
                        : animSet.moveAnimation;

                    _walkState ??= animancerComponent.States.GetOrCreate(walkClip);
                    _walkState.Speed = animSet.walkAnimationSpeedMultiplier;
                    if (!_walkState.IsPlaying) animancerComponent.Play(_walkState, 0.2f);
                }
            }
        }

        public override void ActivateCreature()
        {
            base.ActivateCreature();
            navMeshAgent.enabled = true;
        }

        public override void DeactivateCreature()
        {
            base.DeactivateCreature();
            navMeshAgent.enabled = false;
        }

        public override void OnDeath()
        {
            navMeshAgent.isStopped = true;
            movementLoopFeedbacks?.StopFeedbacks();

            SetLayerRecursively(gameObject, onDeathLayer);

            base.OnDeath();
        }

        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            foreach (var child in obj.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = newLayer;
        }


        public IEnumerator StartAttack(int attackIndex)
        {
            if (IsAttacking) yield break;

            if (attackIndex >= attackInstances.Length) yield break;
            EnemyHitbox soleHitboxCollider = null;
            EnemyHitbox[] multipleHitboxes = null;

            var hasSoleHitbox = attackInstances[attackIndex].AttackHasSingleHitbox;

            if (hasSoleHitbox)
                soleHitboxCollider = attackInstances[attackIndex].attackHitbox;
            else
                multipleHitboxes = attackInstances[attackIndex].multipleHitboxes;

            // var hitboxCollider = attackInstances[attackIndex].attackHitbox;
            var animationClip = attackInstances[attackIndex].attackAnimationClip;
            var animSpeedMult = attackInstances[attackIndex].animationSpeedMultiplier;

            IsAttacking = true;
            IsPlayingCustomAnimation = false;


            AttackState = animancerComponent.Play(animationClip, 0.05f);
            yield return new WaitForSeconds(attackInstances[attackIndex].leadupTime);
            ActivateHitboxes(hasSoleHitbox, soleHitboxCollider, multipleHitboxes, this);
            AttackState.Speed = animSpeedMult;
            yield return new WaitForSeconds(attackInstances[attackIndex].attackDuration);
            DecactivateHitBoxes(hasSoleHitbox, soleHitboxCollider, multipleHitboxes);

            // wait for attack duration
            AttackState.Events(this).OnEnd = () => { AttackState.Speed = 1f; };
            yield return new WaitForSeconds(creatureType.primaryAttackDuration);
            FinishAttack(attackIndex);
        }
        static void ActivateHitboxes(bool hasSoleHitbox, EnemyHitbox soleHitboxCollider, EnemyHitbox[] multipleHitboxes,
            EnemyController owner)
        {
            owner.HasHitPlayerThisAttack = false;
            if (hasSoleHitbox && soleHitboxCollider != null)
                soleHitboxCollider.Activate();
            else if (!hasSoleHitbox && multipleHitboxes != null && multipleHitboxes.Length > 0)
                foreach (var multipleHitbox in multipleHitboxes)
                    if (multipleHitbox != null)
                        multipleHitbox.Activate();
        }

        static void DecactivateHitBoxes(bool hasSoleHitbox, EnemyHitbox soleHitboxCollider,
            EnemyHitbox[] multipleHitboxes)
        {
            if (hasSoleHitbox && soleHitboxCollider != null)
                soleHitboxCollider.Deactivate();
            else if (!hasSoleHitbox && multipleHitboxes != null && multipleHitboxes.Length > 0)
                foreach (var multipleHitbox in multipleHitboxes)
                    if (multipleHitbox != null)
                        multipleHitbox.Deactivate();
        }

        void FinishAttack(int attackIndex)
        {
            if (attackIndex >= attackInstances.Length) return;
            var hitboxCollider = attackInstances[attackIndex].attackHitbox;
            IsAttacking = false;
            if (hitboxCollider != null)
                hitboxCollider.Deactivate();
        }
        public void OnHitPlayer(Collider other, AttackUsed attackUsed)
        {
            if (!other.CompareTag("FirstPersonPlayer")) return;
            if (HasHitPlayerThisAttack) return;

            HasHitPlayerThisAttack = true;

            switch (attackUsed)
            {
                case AttackUsed.Primary:
                    if (attackInstances.Length < 1) return;
                    NPCAttackEvent.Trigger(attackInstances[0].playerAttackData);
                    break;
                case AttackUsed.Secondary:
                    if (attackInstances.Length < 2) return;
                    NPCAttackEvent.Trigger(attackInstances[1].playerAttackData);
                    break;
                case AttackUsed.Third:
                    if (attackInstances.Length < 3) return;
                    NPCAttackEvent.Trigger(attackInstances[2].playerAttackData);
                    break;
            }
        }
    }
}
