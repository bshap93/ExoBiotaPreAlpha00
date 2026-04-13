using Animancer;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace FirstPersonPlayer.Interactable.BioOrganism.Creatures
{
    [RequireComponent(typeof(Blackboard))]
    [RequireComponent(typeof(AnimancerComponent))]
    [DisallowMultipleComponent]
    public class TranslucentFlierController : EnemyController
    {
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] protected float movementSpeedThreshold = 0.1f;
        [Header("Flags")] [SerializeField] protected bool doNotUseIdleState;
        [Header("Feedbacks")] [SerializeField] protected MMFeedbacks movementLoopFeedbacks;

        [SerializeField] protected float walkRunThreshold = 2f;
        AnimancerState _moveBackState;

        AnimancerState _rapidMoveState;


        AnimancerState _slowMoveState;


        // protected virtual void Update()
        // {
        //     if (!IsActivated) return;
        //
        //     var speed = navMeshAgent.velocity.magnitude;
        //     var velocity = navMeshAgent.velocity;
        //
        //     if (speed < movementSpeedThreshold)
        //     {
        //         if (!doNotUseIdleState)
        //             // Idle should NOT interrupt custom animations
        //             if (!IsPlayingCustomAnimation && !IdleState.IsPlaying)
        //                 animancerComponent.Play(IdleState, 0.2f);
        //
        //         movementLoopFeedbacks?.StopFeedbacks();
        //     }
        //     else
        //     {
        //         PlayMovementAnimation(velocity, speed);
        //         IsPlayingCustomAnimation = false;
        //
        //         if (movementLoopFeedbacks != null && !movementLoopFeedbacks.IsPlaying)
        //             movementLoopFeedbacks.PlayFeedbacks();
        //     }
        // }

        public override void ActivateCreature()
        {
            base.ActivateCreature();
            navMeshAgent.enabled = true;
        }

        public override void DeactivateCreature()
        {
            return;
            base.DeactivateCreature();
            navMeshAgent.enabled = false;
        }

        // protected virtual void PlayMovementAnimation(Vector3 worldVelocity, float speed)
        // {
        //     var animSet = creatureType.animationSet;
        //
        //     // Non-humanoid: single movement clip, same as before.
        //     if (!animSet.IsHumanoid)
        //     {
        //         if (!MoveState.IsPlaying)
        //             animancerComponent.Play(MoveState, 0.2f);
        //
        //         return;
        //     }
        //
        //     // Project velocity onto local axes to determine direction.
        //     var localVelocity = transform.InverseTransformDirection(worldVelocity);
        //     var forwardSpeed = localVelocity.z; // positive = forward, negative = backward
        //     var lateralSpeed = localVelocity.x; // positive = right, negative = left
        //
        //     var isMovingBackward = forwardSpeed < -0.3f;
        //
        //     if (isMovingBackward && animSet.walkBackAnimation != null)
        //     {
        //         _moveBackState ??= animancerComponent.States.GetOrCreate(animSet.walkBackAnimation);
        //         _moveBackState.Speed = animSet.walkBackAnimationSpeedMultiplier;
        //         if (!_moveBackState.IsPlaying) animancerComponent.Play(_moveBackState, 0.2f);
        //     }
        //
        //
        //     // Forward movement: walk vs. run.
        //     if (speed >= walkRunThreshold && animSet.runActionClip != null)
        //     {
        //         _rapidMoveState ??= animancerComponent.States.GetOrCreate(animSet.runActionClip.animationClip);
        //         _rapidMoveState.Speed = animSet.runAnimationSpeedMultiplier;
        //         if (!_rapidMoveState.IsPlaying) animancerComponent.Play(_rapidMoveState, 0.2f);
        //     }
        //     else
        //     {
        //         var walkClip = animSet.walkActionClip != null
        //             ? animSet.walkActionClip.animationClip
        //             : animSet.moveAnimation;
        //
        //         _slowMoveState ??= animancerComponent.States.GetOrCreate(walkClip);
        //         _slowMoveState.Speed = animSet.walkAnimationSpeedMultiplier;
        //         if (!_slowMoveState.IsPlaying) animancerComponent.Play(_slowMoveState, 0.2f);
        //     }
        // }
    }
}
