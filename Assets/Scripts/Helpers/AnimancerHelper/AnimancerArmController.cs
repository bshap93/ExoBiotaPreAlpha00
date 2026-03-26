using System;
using Animancer;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using Helpers.Events.NPCs;
using Helpers.ScriptableObjects.Animation;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.AnimancerHelper
{
    public class AnimancerArmController : MonoBehaviour, MMEventListener<NPCAttackEvent>
    {
        [Header("References")] public ToolAnimationSet currentToolAnimationSet;
        public AnimancerComponent animancerComponent;

        [SerializeField] GameObject leftArmReference;

        [Header("Settings")] [Tooltip("Default transition duration for smooth blending")]
        public float defaultTransitionDuration = 0.25f;

        [Tooltip("Transition duration for locomotion changes (idle/walk/run)")]
        public float locomotionTransitionDuration = 0.15f;

        [FormerlySerializedAs("_enableBlockFeedbacks")] [Header("Feedbacks")] [SerializeField]
        MMFeedbacks enableBlockFeedbacks;
        AnimancerState _currentActionState;

        LocomotionState _currentLocoMode = LocomotionState.Idle;


        // Track current states
        AnimancerState _currentLocomotionState;

        bool _isInAimState;
        bool _isInBlockState;
        bool _isNonWeaponToolInUse;


        void LateUpdate()
        {
            // Smoothly fade out the tool/action animation layer when not in use
            var toolLayer = animancerComponent.Layers[1];


            if (!IsPlayingAction())
                toolLayer.Weight = Mathf.MoveTowards(
                    toolLayer.Weight,
                    0f,
                    Time.deltaTime * 8f // fade speed (adjust as needed)
                );
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnValidate()
        {
            // Auto-find AnimancerComponent if not assigned
            if (animancerComponent == null) animancerComponent = GetComponent<AnimancerComponent>();
        }

        public void OnMMEvent(NPCAttackEvent eventType)
        {
            if (eventType.Attack.attackType == NPCAttackType.Melee)
                if (_isInBlockState)
                    PlayBlockHit();
        }


        /// <summary>
        ///     Call this whenever the tool/animation set changes
        /// </summary>
        public void UpdateAnimationSet()
        {
            if (currentToolAnimationSet == null)
            {
                Debug.LogWarning("No ToolAnimationSet assigned!");
                return;
            }

            // Restart with the new animation set
            // This will smoothly transition to the new idle animation
            LoopIdleAnimation();
        }

        public void SetActionState(AnimancerState state)
        {
            _currentActionState = state;
        }

        public void ClearActionState()
        {
            _currentActionState = null;
        }

        public void EnterIntoBlockState()
        {
            if (_isInBlockState) return;

            _isInBlockState = true;

            if (_currentLocoMode == LocomotionState.Idle)
                PlayBlock(LocomotionState.Idle);
            else
                PlayBlock(LocomotionState.Walk);
        }
        void PlayBlock(LocomotionState walk)
        {
            if (currentToolAnimationSet?.blockAnimationClip == null) return;

            if (walk == LocomotionState.Walk)
                _currentLocoMode = LocomotionState.Walk;
            else
                _currentLocoMode = LocomotionState.Idle;

            enableBlockFeedbacks?.PlayFeedbacks();

            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.blockAnimationClip,
                locomotionTransitionDuration
            );

            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }
        public void ReturnFromBlockState()
        {
            if (!_isInBlockState) return;

            _isInBlockState = false;

            // Return to the appropriate non-block locomotion state
            ReturnToLocomotion();
        }


        /// <summary>
        ///     Play the block hit animation when successfully blocking an attack
        /// </summary>
        public void PlayBlockHit()
        {
            if (!_isInBlockState) return;
            if (currentToolAnimationSet?.blockHitAnimationClip == null) return;

            var layer = animancerComponent.Layers[1];
            var state = layer.Play(
                currentToolAnimationSet.blockHitAnimationClip,
                0.05f // Quick transition for responsive blocking
            );

            layer.Weight = 1f;
            SetActionState(state);
            state.Events(this).Clear();

            // After block hit animation, return to block pose
            state.Events(this).OnEnd = () =>
            {
                layer.Weight = 0f;
                ClearActionState();

                // Return to block animation if still blocking
                if (_isInBlockState)
                {
                    _currentLocomotionState = animancerComponent.Play(
                        currentToolAnimationSet.blockAnimationClip,
                        0.1f
                    );

                    _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
                }
                else
                {
                    ReturnToLocomotion();
                }
            };
        }

        public bool EnterIntoInjectionAnimation()
        {
            if (_isNonWeaponToolInUse) return false;
            if (IsPlayingAction()) return false;

            _isNonWeaponToolInUse = true;

            if (_currentLocoMode == LocomotionState.Idle)
            {
                PlayInject(LocomotionState.Idle);
                return true;
            }

            PlayInject(LocomotionState.Walk);
            return true;
        }

        void PlayInject(LocomotionState loco)
        {
            if (currentToolAnimationSet?.secondaryUseAnimation == null) return;

            var layer = animancerComponent.Layers[1];
            var state = layer.Play(
                currentToolAnimationSet.secondaryUseAnimation,
                defaultTransitionDuration
            );

            // state.Time = 0f;

            layer.Weight = 1f;
            SetActionState(state);
            state.Events(this).Clear();

            state.Events(this).OnEnd = () =>
            {
                layer.Weight = 0f;
                ClearActionState();
                _isNonWeaponToolInUse = false; // ← unlock for future inject calls
                ReturnToLocomotion();
            };

            // if (loco == LocomotionState.Idle)
            //     _currentLocoMode = LocomotionState.Idle;
            // else
            //     _currentLocoMode = LocomotionState.Walk;


            // _currentLocomotionState = animancerComponent.Play(
            //     currentToolAnimationSet.secondaryUseAnimation,
            //     locomotionTransitionDuration
            // );
            //
            // _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }


        enum LocomotionState
        {
            Idle,
            Walk,
            Run
        }

        #region Locomotion Animations

        /// <summary>
        ///     Play and loop the idle animation
        /// </summary>
        public void LoopIdleAnimation()
        {
            if (currentToolAnimationSet?.idleAnimation == null) return;

            _currentLocoMode = LocomotionState.Idle;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.idleAnimation,
                locomotionTransitionDuration
            );

            // Make sure idle loops
            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Play and loop the walk animation
        /// </summary>
        public void LoopWalkAnimation()
        {
            if (currentToolAnimationSet?.walkAnimation == null) return;

            _currentLocoMode = LocomotionState.Walk;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.walkAnimation,
                locomotionTransitionDuration
            );

            // Make walk animation loop
            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Play and loop the run animation
        /// </summary>
        public void LoopRunAnimation()
        {
            if (currentToolAnimationSet?.runAnimation == null) return;

            _currentLocoMode = LocomotionState.Run;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.runAnimation,
                locomotionTransitionDuration
            );

            // Make run animation loop
            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Smoothly transition from idle to walk
        /// </summary>
        public void MoveFromIdleToWalk()
        {
            if (_currentLocoMode != LocomotionState.Walk) LoopWalkAnimation();
        }

        /// <summary>
        ///     Smoothly transition back to idle
        /// </summary>
        public void MoveToIdle()
        {
            LoopIdleAnimation();
        }

        public void UpdateLocomotion(bool isMoving, bool isRunning = false)
        {
            if (IsPlayingAction())
                return; // Don't interrupt action animations

            if (_isInAimState)
            {
                // In aim state, only toggle between aim idle and aim walk
                // (no running while aiming)
                if (!isMoving)
                {
                    if (_currentLocoMode != LocomotionState.Idle)
                        PlayAimIdle();
                }
                else
                {
                    if (_currentLocoMode != LocomotionState.Walk)
                        PlayAimWalk();
                }
            }
            else if (_isInBlockState)
            {
            }
            else
            {
                // Normal locomotion
                if (!isMoving)
                {
                    if (_currentLocoMode != LocomotionState.Idle)
                        MoveToIdle();
                }
                else if (isRunning)
                {
                    if (_currentLocoMode != LocomotionState.Run)
                        LoopRunAnimation();
                }
                else
                {
                    if (_currentLocoMode != LocomotionState.Walk)
                        LoopWalkAnimation();
                }
            }
        }

        #endregion

        #region Tool Use Animations

        /// <summary>
        ///     Play the tool use sequence: begin -> during (loop) -> end
        /// </summary>
        public void PlayToolUseSequence(Action onComplete = null)
        {
            if (currentToolAnimationSet == null) return;

            var layer = animancerComponent.Layers[1];

            // BEGIN animation
            var clip = currentToolAnimationSet.beginUseAnimation;
            if (clip == null)
            {
                PlayToolDuringUse();
                return;
            }

            var state = layer.Play(clip, defaultTransitionDuration);
            layer.Weight = 1f;

            SetActionState(state);
            state.Events(this).Clear();

            state.Events(this).OnEnd = () => { PlayToolDuringUse(); };
        }
        /// <summary>
        ///     Play the looping "during use" animation
        /// </summary>
        void PlayToolDuringUse()
        {
            if (currentToolAnimationSet?.duringUseAnimationLoopable == null)
                return;

            var layer = animancerComponent.Layers[1];
            var state = layer.Play(
                currentToolAnimationSet.duringUseAnimationLoopable,
                defaultTransitionDuration
            );

            layer.Weight = 1f;
            SetActionState(state);
            state.Events(this).Clear();

            state.Events(this).OnEnd = () =>
            {
                // Looping clip → keep at end until EndToolUse is called
                state.Time = state.Length; // freeze
            };
        }

        /// <summary>
        ///     End the tool use and transition back to locomotion
        /// </summary>
        public void EndToolUse(Action onComplete = null)
        {
            var clip = currentToolAnimationSet?.endUseAnimation;
            var layer = animancerComponent.Layers[1];

            if (clip == null)
            {
                layer.Weight = 0f;
                ClearActionState();
                ReturnToLocomotion();
                onComplete?.Invoke();
                return;
            }

            var state = layer.Play(clip, defaultTransitionDuration);
            layer.Weight = 1f;

            SetActionState(state);
            state.Events(this).Clear();

            state.Events(this).OnEnd = () =>
            {
                layer.Weight = 0f;
                ClearActionState();
                ReturnToLocomotion();
                onComplete?.Invoke();
            };
        }

        /// <summary>
        ///     Play a one-shot tool use animation (begin and end together)
        /// </summary>
        public void PlayToolUseOneShot(Action onComplete = null, float speedMultiplier = 1f)
        {
            if (currentToolAnimationSet?.beginUseAnimation == null) return;

            var state = animancerComponent.Play(
                currentToolAnimationSet.beginUseAnimation,
                defaultTransitionDuration
            );

            state.Speed = speedMultiplier;

            state.Events(this).OnEnd = () =>
            {
                ReturnToLocomotion();
                onComplete?.Invoke();
            };
        }

        #endregion

        #region Equipment Animations

        public void PlayEquipAnimation(Action onComplete = null)
        {
            // If there's no equip animation, just return
        }

        public void PlayUnequipAnimation(Action onComplete = null)
        {
            // If there's no unequip animation, just return
        }

        /// <summary>
        ///     Play the aim shot animation on Layer 1
        /// </summary>
        public void PlayAimShot(Action onComplete = null)
        {
            if (currentToolAnimationSet?.aimShotAnimation == null) return;

            var layer = animancerComponent.Layers[1];
            var state = layer.Play(
                currentToolAnimationSet.aimShotAnimation,
                0.05f // Quick transition for responsive shooting
            );

            state.Time = 0f;
            state.Speed = 1f;

            layer.Weight = 1f;
            SetActionState(state);
            state.Events(this).Clear();

            state.Events(this).OnEnd = () =>
            {
                ClearActionState();
                onComplete?.Invoke();
            };
        }

        /// <summary>
        ///     Play the non-aim shot animation
        /// </summary>
        public void PlayNonAimShot(Action onComplete = null)
        {
            if (currentToolAnimationSet?.nonAimShotAnimation == null) return;

            var layer = animancerComponent.Layers[1];
            var state = layer.Play(
                currentToolAnimationSet.nonAimShotAnimation,
                0.1f
            );

            state.Time = 0f;
            state.Speed = 1f;

            layer.Weight = 1f;


            SetActionState(state);
            state.Events(this).Clear();

            state.Events(this).OnEnd = () =>
            {
                ClearActionState();
                ReturnToLocomotion();
                onComplete?.Invoke();
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        ///     Return to the appropriate locomotion animation based on current mode
        /// </summary>
        public void ReturnToLocomotion()
        {
            switch (_currentLocoMode)
            {
                case LocomotionState.Idle:
                    LoopIdleAnimation();
                    break;
                case LocomotionState.Walk:
                    LoopWalkAnimation();
                    break;
                case LocomotionState.Run:
                    LoopRunAnimation();
                    break;
            }
        }
        /// <summary>
        ///     Check if currently playing a tool action animation
        /// </summary>
        public bool IsPlayingAction()
        {
            return _currentActionState != null && _currentActionState.IsPlaying;
        }
        /// <summary>
        ///     Stop all animations
        /// </summary>
        public void StopAll()
        {
            animancerComponent.Stop();
            _currentLocomotionState = null;
            _currentActionState = null;
        }

        #endregion

        #region Aim State Management

        /// <summary>
        ///     Enter aim state - transitions to aim idle animation
        /// </summary>
        public void EnterIntoAimState()
        {
            if (_isInAimState) return;

            _isInAimState = true;

            // Transition to appropriate aim animation based on current movement
            if (_currentLocoMode == LocomotionState.Idle)
                PlayAimIdle();
            else
                PlayAimWalk();
        }

        /// <summary>
        ///     Return from aim state - transitions back to normal locomotion
        /// </summary>
        public void ReturnFromAimState()
        {
            if (!_isInAimState) return;

            _isInAimState = false;

            // Return to the appropriate non-aim locomotion state
            ReturnToLocomotion();
        }

        /// <summary>
        ///     Play aim idle animation
        /// </summary>
        void PlayAimIdle()
        {
            if (currentToolAnimationSet?.aimIdleAnimation == null) return;

            _currentLocoMode = LocomotionState.Idle;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.aimIdleAnimation,
                locomotionTransitionDuration
            );

            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Play aim walk animation
        /// </summary>
        void PlayAimWalk()
        {
            if (currentToolAnimationSet?.aimWalkAnimation == null) return;

            _currentLocoMode = LocomotionState.Walk;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.aimWalkAnimation,
                locomotionTransitionDuration
            );

            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Check if currently in aim state
        /// </summary>
        public bool IsInAimState()
        {
            return _isInAimState;
        }

        #endregion
    }
}
