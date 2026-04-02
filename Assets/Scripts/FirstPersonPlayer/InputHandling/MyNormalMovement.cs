using System;
using Domains.Player.Events;
using Events;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.Events.Status;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using ThirdParty.Character_Controller_Pro.Implementation.Scripts.Character.States;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Static;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace FirstPersonPlayer.InputHandling
{
    [MMRequiresConstantRepaint]
    [AddComponentMenu("Character Controller Pro/Demo/Character/States/Normal Movement")]
    public class MyNormalMovement : CharacterState, MMEventListener<PointedObjectEvent>, MMEventListener<MyUIEvent>,
        MMEventListener<PlayerDeathEvent>
    {
        public enum JumpResult
        {
            Invalid,
            Grounded,
            NotGrounded
        }

        [Header("Aim Movement")] [SerializeField]
        float aimSpeedMultiplier = 0.5f; // 50% speed while aiming
        [FormerlySerializedAs("playerStatsManager")] [SerializeField]
        PlayerMutableStatsManager playerMutableStatsManager;
        [SerializeField] AttributesManager attributesManager;


        [FormerlySerializedAs("animancerRightArmController")]
        public AnimancerArmController animancerArmController;


        [Space(10)] public PlanarMovementParameters planarMovementParameters = new();

        public VerticalMovementParameters verticalMovementParameters = new();

        public CrouchParameters crouchParameters = new();

        public LookingDirectionParameters lookingDirectionParameters = new();

        public PlayerInteraction playerInteraction;

        // [FormerlySerializedAs("ForwardTextureDetector")]
        // public TerrainLayerDetector forwardTerrainLayerDetector;

        [Header("Animation")] [SerializeField] protected string groundedParameter = "Grounded";

        [SerializeField] protected string stableParameter = "Stable";

        [SerializeField] protected string verticalSpeedParameter = "VerticalSpeed";

        [SerializeField] protected string planarSpeedParameter = "PlanarSpeed";

        [SerializeField] protected string horizontalAxisParameter = "HorizontalAxis";

        [SerializeField] protected string verticalAxisParameter = "VerticalAxis";

        [SerializeField] protected string heightParameter = "Height";

        [Header("Jet Pack")] [SerializeField] protected float jetPackDuration = 0.1f;

        [SerializeField] MMFeedbacks jetPackFeedbacks;

        [SerializeField] public float jetPackSpeedMultiplier = 1.22f;

        [Header("Landing Feedbacks")] [SerializeField]
        protected MMFeedbacks softLandingFeedbacks;
        [SerializeField] protected MMFeedbacks hardLandingFeedbacks;
        [SerializeField] protected MMFeedbacks heavyLandingFeedbacks;

        [SerializeField] protected MMFeedbacks waterLandingFeedbacks;

        [SerializeField] protected MMFeedbacks jumpStartFeedbacks;


        // Add these new fields for fall damage
        [Header("Fall Damage")] [SerializeField]
        protected bool enableFallDamage = true;
        [SerializeField] protected float increasedMinFallDmgSpdPerToughness = 1.25f;
        [SerializeField]
        protected float fractionAmtSpeedIncreasePerAgility = 0.2f; // Each point of Agility increases speed by 10%

        [SerializeField] protected float minimumFallDamageSpeed = 10f;
        [SerializeField] protected float fallDamageMultiplier = 1f;


        [SerializeField] float jetPackActivationDelay = 0.3f; // Time in seconds to hold before jetpack activates
        public bool isBlocking;


        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        // Near your other fields
        Vector3 _attackLungeVelocity;

        bool _isDead;

        int _toughness;

        protected PlanarMovementParameters.PlanarMovementProperties currentMotion;
        protected float currentPlanarSpeedLimit;

        protected bool groundedJumpAvailable;
        protected bool isAllowedToCancelJump;
        protected bool isCrouched;

        protected bool IsFalling;

        // public TextureDetector textureDetector;

        bool isJetpackEquipped;

        bool isUsingAUI;
        // [SerializeField] private float maxSpeed = 5f; // used for scaling

        JetPackBehavior jetPackBehavior;

        // private float jetPackButtonHoldTime;
        protected Vector3 jumpDirection;


        protected MaterialController materialController;

        // Track the maximum fall velocity for damage calculation
        protected float maxFallSpeed;
        protected int notGroundedJumpsLeft;
        bool reducedAirControlFlag;
        float reducedAirControlInitialTime;
        float reductionDuration = 0.5f;
        protected float targetHeight = 1f;

        protected Vector3 targetLookingDirection;

        protected bool wantToCrouch;
        protected bool wantToRun;

        public bool IsRunning => wantToRun && CharacterActor.PlanarVelocity.sqrMagnitude > 0.01f;
        public bool IsMoving => CharacterActor.PlanarVelocity.sqrMagnitude > 0.01f;

        public bool IsAiming { get; set; }

        /// <summary>
        ///     Gets/Sets the useGravity toggle. Use this property to enable/disable the effect of gravity on the character.
        /// </summary>
        /// <value></value>
        public bool UseGravity
        {
            get => verticalMovementParameters.useGravity;
            set => verticalMovementParameters.useGravity = value;
        }


        protected bool UnstableGroundedJumpAvailable => !verticalMovementParameters.canJumpOnUnstableGround &&
                                                        CharacterActor.CurrentState ==
                                                        CharacterActorState.UnstableGrounded;

        protected override void Awake()
        {
            base.Awake();

            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;

            materialController = this.GetComponentInBranch<CharacterActor, MaterialController>();

            if (playerInteraction == null)
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();

            jetPackBehavior = GetComponent<JetPackBehavior>();
        }

        protected override void Start()
        {
            base.Start();

            targetHeight = CharacterActor.DefaultBodySize.y;

            var minCrouchHeightRatio = CharacterActor.BodySize.x / CharacterActor.BodySize.y;
            crouchParameters.heightRatio = Mathf.Max(minCrouchHeightRatio, crouchParameters.heightRatio);

            playerMutableStatsManager = PlayerMutableStatsManager.Instance;
            attributesManager = AttributesManager.Instance;

            _toughness = attributesManager != null ? attributesManager.Toughness : 1;
        }

        protected virtual void OnEnable()
        {
            CharacterActor.OnTeleport += OnTeleport;
            this.MMEventStartListening<PointedObjectEvent>();
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<PlayerDeathEvent>();
        }

        protected virtual void OnDisable()
        {
            CharacterActor.OnTeleport -= OnTeleport;
            this.MMEventStopListening<PointedObjectEvent>();
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<PlayerDeathEvent>();
        }

        protected virtual void OnValidate()
        {
            verticalMovementParameters.OnValidate();
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiActionType == UIActionType.Open
               )
                isUsingAUI = true;
            else if (eventType.uiActionType == UIActionType.Close
                    )
                isUsingAUI = false;
        }

        public void OnMMEvent(PlayerDeathEvent eventType)
        {
            _isDead = true;
            enableFallDamage = false; // disable fall damage on death
            Debug.Log("Player has died. NormalMovement state will stop processing movement.");
        }

        public void OnMMEvent(PointedObjectEvent eventType)
        {
            if (eventType.EventType == PointedObjectEventType.PointedObjectChanged)
            {
            }
        }

        public override string GetInfo()
        {
            return
                "This state serves as a multi purpose movement based state. It is responsible for handling gravity and jump, walk and run, crouch, " +
                "react to the different material properties, etc. Basically it covers all the common movements involved " +
                "in a typical game, from a 3D platformer to a first person walking simulator.";
        }

        void OnTeleport(Vector3 position, Quaternion rotation)
        {
            targetLookingDirection = CharacterActor.Forward;
            isAllowedToCancelJump = false;

            // Reset fall tracking when teleporting
            IsFalling = false;
            maxFallSpeed = 0f;
        }


        public override void CheckExitTransition()
        {
            if (InputService.IsApplyEffectHeld()
                // && !PlayerFuelManager.IsPlayerOutOfFuel()
               )
                if (playerInteraction == null)
                    return;
        }


        public override void ExitBehaviour(float dt, CharacterState toState)
        {
            reducedAirControlFlag = false;
        }


        /// <summary>
        ///     Reduces the amount of acceleration and deceleration (not grounded state) until the character reaches the apex of
        ///     the jump
        ///     (vertical velocity close to zero). This can be useful to prevent the character from accelerating/decelerating too
        ///     quickly (e.g. right after performing a wall jump).
        /// </summary>
        public void ReduceAirControl(float reductionDuration = 0.5f)
        {
            reducedAirControlFlag = true;
            reducedAirControlInitialTime = Time.time;
            this.reductionDuration = reductionDuration;
        }

        void SetMotionValues(Vector3 targetPlanarVelocity)
        {
            var angleCurrentTargetVelocity = Vector3.Angle(CharacterActor.PlanarVelocity, targetPlanarVelocity);

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    currentMotion.acceleration = planarMovementParameters.stableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.stableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier =
                        planarMovementParameters.stableGroundedAngleAccelerationBoost.Evaluate(
                            angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.UnstableGrounded:
                    currentMotion.acceleration = planarMovementParameters.unstableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.unstableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier =
                        planarMovementParameters.unstableGroundedAngleAccelerationBoost.Evaluate(
                            angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.NotGrounded:

                    if (reducedAirControlFlag)
                    {
                        var time = Time.time - reducedAirControlInitialTime;
                        if (time <= reductionDuration)
                        {
                            currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration /
                                reductionDuration * time;

                            currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration /
                                reductionDuration * time;
                        }
                        else
                        {
                            reducedAirControlFlag = false;

                            currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                            currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                        }
                    }
                    else
                    {
                        currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                        currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                    }

                    currentMotion.angleAccelerationMultiplier =
                        planarMovementParameters.notGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;
            }


            // Material values
            if (materialController != null)
            {
                if (CharacterActor.IsGrounded)
                {
                    currentMotion.acceleration *= materialController.CurrentSurface.accelerationMultiplier *
                                                  materialController.CurrentVolume.accelerationMultiplier;

                    currentMotion.deceleration *= materialController.CurrentSurface.decelerationMultiplier *
                                                  materialController.CurrentVolume.decelerationMultiplier;
                }
                else
                {
                    currentMotion.acceleration *= materialController.CurrentVolume.accelerationMultiplier;
                    currentMotion.deceleration *= materialController.CurrentVolume.decelerationMultiplier;
                }
            }
        }

        /// <summary>
        ///     Processes the lateral movement of the character (stable and unstable state), that is, walk, run, crouch, etc.
        ///     This movement is tied directly to the "movement" character action.
        /// </summary>
        protected virtual void ProcessPlanarMovement(float dt)
        {
            //SetMotionValues();

            var speedMultiplier = materialController != null
                ? materialController.CurrentSurface.speedMultiplier * materialController.CurrentVolume.speedMultiplier
                : 1f;

            // Based on Agility attribute (from AttributesManager)
            if (attributesManager != null)
            {
                var agilityMultiplier = 1f + (attributesManager.Agility - 1) * fractionAmtSpeedIncreasePerAgility;
                speedMultiplier *= agilityMultiplier;
                // Debug.Log("Speed multipliler: " + speedMultiplier);
            }


            var needToAccelerate =
                CustomUtilities.Multiply(CharacterStateController.InputMovementReference, currentPlanarSpeedLimit)
                    .sqrMagnitude >= CharacterActor.PlanarVelocity.sqrMagnitude;

            Vector3 targetPlanarVelocity = default;
            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:

                    if (CharacterActor.WasGrounded)
                        currentPlanarSpeedLimit = Mathf.Max(
                            CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

                    // Aim speed reduction
                    if (IsAiming) currentPlanarSpeedLimit *= aimSpeedMultiplier;

                    targetPlanarVelocity = CustomUtilities.Multiply(
                        CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.StableGrounded:


                    // Run ------------------------------------------------------------
                    if (planarMovementParameters.runInputMode == InputMode.Toggle)
                    {
                        if (CharacterActions.run.Started)
                            wantToRun = !wantToRun;
                    }
                    else
                    {
                        if (playerMutableStatsManager.CurrentStamina > 0)
                            wantToRun = CharacterActions.run.value;
                        else
                            wantToRun = false;
                        // cannotRunFeedbacks?.PlayFeedbacks();
                    }

                    if (wantToCrouch || !planarMovementParameters.canRun)
                        wantToRun = false;


                    if (isCrouched)
                        currentPlanarSpeedLimit =
                            planarMovementParameters.baseSpeedLimit * crouchParameters.speedMultiplier;
                    else
                        currentPlanarSpeedLimit = wantToRun
                            ? planarMovementParameters.boostSpeedLimit
                            : planarMovementParameters.baseSpeedLimit;

                    // Aim speed reduction
                    if (IsAiming) currentPlanarSpeedLimit *= aimSpeedMultiplier;

                    targetPlanarVelocity = CustomUtilities.Multiply(
                        CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.UnstableGrounded:

                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    // Aim speed reduction
                    if (IsAiming) currentPlanarSpeedLimit *= aimSpeedMultiplier;

                    targetPlanarVelocity = CustomUtilities.Multiply(
                        CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);


                    break;
            }

            SetMotionValues(targetPlanarVelocity);


            var acceleration = currentMotion.acceleration;


            if (needToAccelerate)
                acceleration *= currentMotion.angleAccelerationMultiplier;
            else
                acceleration = currentMotion.deceleration;

            CharacterActor.PlanarVelocity = Vector3.MoveTowards(
                CharacterActor.PlanarVelocity,
                targetPlanarVelocity,
                acceleration * dt
            );

            // Attack lunge — applied after normal movement so it doesn't get overwritten
            if (_attackLungeVelocity.sqrMagnitude > 0.001f) CharacterActor.PlanarVelocity += _attackLungeVelocity;
        }

        protected virtual void UpdateArmLocomotion()
        {
            if (animancerArmController == null) return;

            // Calculate if we're moving
            var isMoving = CharacterActor.PlanarVelocity.magnitude > 0.1f;


            // Check if we're running (based on your existing wantToRun logic)
            var isRunning = wantToRun && isMoving;

            // Update the arm animations
            animancerArmController.UpdateLocomotion(isMoving, isRunning);
        }


        protected virtual void ProcessGravity(float dt)
        {
            if (!verticalMovementParameters.useGravity)
                return;


            verticalMovementParameters.UpdateParameters();


            var gravityMultiplier = 1f;

            if (materialController != null)
                gravityMultiplier = CharacterActor.LocalVelocity.y >= 0
                    ? materialController.CurrentVolume.gravityAscendingMultiplier
                    : materialController.CurrentVolume.gravityDescendingMultiplier;

            var gravity = gravityMultiplier * verticalMovementParameters.gravity;


            if (!CharacterActor.IsStable)
                CharacterActor.VerticalVelocity += CustomUtilities.Multiply(-CharacterActor.Up, gravity, dt);
        }

        JumpResult CanJump()
        {
            var jumpResult = JumpResult.Invalid;

            if (!verticalMovementParameters.canJump)
                return jumpResult;

            if (isCrouched)
                return jumpResult;

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime &&
                        groundedJumpAvailable)
                        jumpResult = JumpResult.Grounded;

                    break;
                case CharacterActorState.NotGrounded:

                    if (CharacterActions.jump.Started)
                    {
                        // First check if the "grounded jump" is available. If so, execute a "coyote jump".
                        if (CharacterActor.NotGroundedTime <= verticalMovementParameters.postGroundedJumpTime &&
                            groundedJumpAvailable)
                            jumpResult = JumpResult.Grounded;
                        else if (notGroundedJumpsLeft != 0) // Do a 'not grounded' jump
                            jumpResult = JumpResult.NotGrounded;
                    }

                    break;
                case CharacterActorState.UnstableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime &&
                        verticalMovementParameters.canJumpOnUnstableGround)
                        jumpResult = JumpResult.Grounded;

                    break;
            }

            return jumpResult;
        }


        protected virtual void ProcessJump(float dt)
        {
            ProcessRegularJump(dt);
            ProcessJumpDown(dt);
        }

        protected virtual void ProcessJetPack(float dt)
        {
            // Track how long the jetpack button has been held
            if (CharacterActions.jetPack.value)
            {
                jetPackBehavior.UpdateHoldTime(dt);

                // Only activate jetpack if button has been held long enough
                if (jetPackBehavior.GetHoldTime() >= jetPackActivationDelay)
                {
                    CharacterActor.VerticalVelocity = jetPackBehavior.ApplyJetpackLift(
                        CharacterActor.VerticalVelocity,
                        CharacterActor.Up,
                        targetHeight,
                        jetPackDuration,
                        jetPackSpeedMultiplier
                    );

                    jetPackBehavior.TryTriggerJetPackEffect();
                    jetPackFeedbacks?.PlayFeedbacks();

                    // jetPackBehavior.JetPackBehaviorMethod();
                    jetPackFeedbacks?.PlayFeedbacks();
                }
            }
            else
            {
                // Reset the hold timer when button is released
                jetPackBehavior.ResetHoldTime();
            }

            CharacterActor.SetYaw(CharacterActor.PlanarVelocity);
        }


        void ProcessVerticalMovement(float dt)
        {
            // Track falling state and maximum fall speed
            if (enableFallDamage)
            {
                // If we're falling (negative vertical velocity in the up direction)
                var currentFallSpeed = -Vector3.Dot(CharacterActor.Velocity, CharacterActor.Up);

                if (currentFallSpeed > 0 && !CharacterActor.IsGrounded)
                {
                    IsFalling = true;
                    // Track the maximum fall speed during this fall
                    maxFallSpeed = Mathf.Max(maxFallSpeed, currentFallSpeed);
                }

                // Check for landing impact
                if (IsFalling && CharacterActor.IsGrounded)
                {
                    HandleFallDamage();
                    // Reset fall tracking
                    IsFalling = false;
                    maxFallSpeed = 0f;
                }
            }

            ProcessGravity(dt);
            ProcessJump(dt);
            // if (!isJetpackEquipped) isJetpackEquipped = ProgressionManager.IsObjectiveCollected("Jetpack");
            if (isJetpackEquipped)
                ProcessJetPack(dt);
        }

        // New method to handle fall damage
        protected virtual void HandleFallDamage()
        {
            var toughness = attributesManager.Toughness;
            var workingMinimumFallDamageSpeed =
                minimumFallDamageSpeed + toughness * increasedMinFallDmgSpdPerToughness;

            var percentHardnessOfLanding = Mathf.InverseLerp(
                workingMinimumFallDamageSpeed, minimumFallDamageSpeed + 20f, maxFallSpeed);

            if (percentHardnessOfLanding < 0.25f)
                softLandingFeedbacks?.PlayFeedbacks();
            else if (percentHardnessOfLanding < 0.75f)
                hardLandingFeedbacks?.PlayFeedbacks();
            else
                heavyLandingFeedbacks?.PlayFeedbacks();

            if (!enableFallDamage)
                return;

            // If fall speed exceeds the threshold, apply damage
            if (maxFallSpeed > minimumFallDamageSpeed)
            {
                // Calculate damage - simple linear model based on speed beyond the threshold
                var excessSpeed = maxFallSpeed - minimumFallDamageSpeed;
                var damage = excessSpeed * fallDamageMultiplier;

                // Call the damage event
                Debug.Log("Fall damage applied: " + damage);
                // Severity between 0 and 1
                var percent = Mathf.InverseLerp(minimumFallDamageSpeed, minimumFallDamageSpeed + 20f, maxFallSpeed);
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentHealth,
                    PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    damage, 0f, PlayerStatsEvent.StatChangeCause.FallDamage,
                    percent, sourcePosition: CharacterActor.Position
                );

                CameraEvent.Trigger(CameraEventType.CameraShake, 0.5f, 0.5f);
            }
        }


        public override void EnterBehaviour(float dt, CharacterState fromState)
        {
            targetLookingDirection = CharacterActor.Forward;
            CharacterActor.alwaysNotGrounded = false;

            // Reset fall tracking
            IsFalling = false;
            maxFallSpeed = 0f;

            // Grounded jump
            groundedJumpAvailable = false;
            if (CharacterActor.IsGrounded)
                if (verticalMovementParameters.canJumpOnUnstableGround || CharacterActor.IsStable)
                    groundedJumpAvailable = true;

            // Wallside to NormalMovement transition
            if (fromState == CharacterStateController.GetState<WallSlide>())
            {
                // "availableNotGroundedJumps + 1" because the update code will consume one jump!
                notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps + 1;

                // Reduce the amount of air control (acceleration and deceleration) for 0.5 seconds.
                ReduceAirControl();
            }

            currentPlanarSpeedLimit = Mathf.Max(
                CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

            CharacterActor.UseRootMotion = false;
        }

        protected virtual void HandleRotation(float dt)
        {
            HandleLookingDirection(dt);
        }

        void HandleLookingDirection(float dt)
        {
            if (!lookingDirectionParameters.changeLookingDirection)
                return;

            switch (lookingDirectionParameters.lookingDirectionMode)
            {
                case LookingDirectionParameters.LookingDirectionMode.Movement:

                    switch (CharacterActor.CurrentState)
                    {
                        case CharacterActorState.NotGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.notGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.StableGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.stableGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.UnstableGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.unstableGroundedLookingDirectionMode);

                            break;
                    }

                    break;

                case LookingDirectionParameters.LookingDirectionMode.ExternalReference:

                    if (!CharacterActor.CharacterBody.Is2D)
                        targetLookingDirection = CharacterStateController.MovementReferenceForward;

                    break;

                case LookingDirectionParameters.LookingDirectionMode.Target:

                    targetLookingDirection = lookingDirectionParameters.target.position - CharacterActor.Position;
                    targetLookingDirection.Normalize();

                    break;
            }

            var targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, targetLookingDirection);
            var currentDeltaRotation = Quaternion.Slerp(
                Quaternion.identity, targetDeltaRotation, lookingDirectionParameters.speed * dt);

            if (CharacterActor.CharacterBody.Is2D)
                CharacterActor.SetYaw(targetLookingDirection);
            else
                CharacterActor.SetYaw(currentDeltaRotation * CharacterActor.Forward);
        }

        void SetTargetLookingDirection(
            LookingDirectionParameters.LookingDirectionMovementSource lookingDirectionMode)
        {
            if (lookingDirectionMode == LookingDirectionParameters.LookingDirectionMovementSource.Input)
            {
                if (CharacterStateController.InputMovementReference != Vector3.zero)
                    targetLookingDirection = CharacterStateController.InputMovementReference;
                else
                    targetLookingDirection = CharacterActor.Forward;
            }
            else
            {
                if (CharacterActor.PlanarVelocity != Vector3.zero)
                    targetLookingDirection = Vector3.ProjectOnPlane(CharacterActor.PlanarVelocity, CharacterActor.Up);
                else
                    targetLookingDirection = CharacterActor.Forward;
            }
        }


        public override void UpdateBehaviour(float dt)
        {
            if (isUsingAUI)
            {
                CharacterActor.PlanarVelocity = Vector3.zero;
                CharacterActor.VerticalVelocity = Vector3.zero;
                UpdateArmLocomotion();
                return;
            }

            if (_isDead)
            {
                CharacterActor.PlanarVelocity = Vector3.zero;
                CharacterActor.VerticalVelocity = Vector3.zero;
                // UpdateArmLocomotion();
                return;
            }

            HandleSize(dt);
            HandleVelocity(dt);
            HandleRotation(dt);
            UpdateArmLocomotion();
        }


        public override void PreCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            // if (!CharacterActor.IsAnimatorValid())
            //     return;

            // CharacterStateController.Animator.SetBool(groundedParameter, CharacterActor.IsGrounded);
            // CharacterStateController.Animator.SetBool(stableParameter, CharacterActor.IsStable);
            // CharacterStateController.Animator.SetFloat(horizontalAxisParameter, CharacterActions.movement.value.x);
            // CharacterStateController.Animator.SetFloat(verticalAxisParameter, CharacterActions.movement.value.y);
            // CharacterStateController.Animator.SetFloat(heightParameter, CharacterActor.BodySize.y);
        }

        public override void PostCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            // Parameters associated with velocity are sent after the simulation.
            // The PostSimulationUpdate (CharacterActor) might update velocity once more (e.g. if a "bad step" has been detected).
            // CharacterStateController.Animator.SetFloat(verticalSpeedParameter, CharacterActor.LocalVelocity.y);
            // CharacterStateController.Animator.SetFloat(planarSpeedParameter, CharacterActor.PlanarVelocity.magnitude);
        }

        protected virtual void HandleSize(float dt)
        {
            // Get the crouch input state 
            if (crouchParameters.enableCrouch)
            {
                if (crouchParameters.inputMode == InputMode.Toggle)
                {
                    if (CharacterActions.crouch.Started)
                        wantToCrouch = !wantToCrouch;
                }
                else
                {
                    wantToCrouch = CharacterActions.crouch.value;
                }

                if (!crouchParameters.notGroundedCrouch && !CharacterActor.IsGrounded)
                    wantToCrouch = false;

                if (CharacterActor.IsGrounded && wantToRun)
                    wantToCrouch = false;
            }
            else
            {
                wantToCrouch = false;
            }

            if (wantToCrouch)
                Crouch(dt);
            else
                StandUp(dt);
        }

        void Crouch(float dt)
        {
            var sizeReferenceType = CharacterActor.IsGrounded
                ? CharacterActor.SizeReferenceType.Bottom
                : crouchParameters.notGroundedReference;

            var validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y * crouchParameters.heightRatio,
                crouchParameters.sizeLerpSpeed * dt,
                sizeReferenceType);

            if (validSize)
                isCrouched = true;
        }

        void StandUp(float dt)
        {
            var sizeReferenceType = CharacterActor.IsGrounded
                ? CharacterActor.SizeReferenceType.Bottom
                : crouchParameters.notGroundedReference;

            var validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y,
                crouchParameters.sizeLerpSpeed * dt,
                sizeReferenceType);

            if (validSize)
                isCrouched = false;
        }


        protected virtual void HandleVelocity(float dt)
        {
            ProcessVerticalMovement(dt);
            ProcessPlanarMovement(dt);
        }

        public void SetAttackLungeVelocity(Vector3 velocity)
        {
            _attackLungeVelocity = velocity;
        }

        #region Events

        /// <summary>
        ///     Event triggered when the character jumps.
        /// </summary>
        public event Action OnJumpPerformed;

        /// <summary>
        ///     Event triggered when the character jumps from the ground.
        /// </summary>
        public event Action<bool> OnGroundedJumpPerformed;

        /// <summary>
        ///     Event triggered when the character jumps while.
        /// </summary>
        public event Action<int> OnNotGroundedJumpPerformed;

        #endregion

        #region JumpDown

        protected virtual bool ProcessJumpDown(float dt)
        {
            if (!verticalMovementParameters.canJumpDown)
                return false;

            if (!CharacterActor.IsStable)
                return false;

            if (!CharacterActor.IsGroundAOneWayPlatform)
                return false;

            if (verticalMovementParameters.filterByTag)
                if (!CharacterActor.GroundObject.CompareTag(verticalMovementParameters.jumpDownTag))
                    return false;

            if (!ProcessJumpDownAction())
                return false;

            JumpDown(dt);

            return true;
        }


        protected virtual bool ProcessJumpDownAction()
        {
            return isCrouched && CharacterActions.jump.Started;
        }


        protected virtual void JumpDown(float dt)
        {
            var groundDisplacementExtraDistance = 0f;

            var groundDisplacement = CustomUtilities.Multiply(CharacterActor.GroundVelocity, dt);

            if (!CharacterActor.IsGroundAscending)
                groundDisplacementExtraDistance = groundDisplacement.magnitude;

            CharacterActor.ForceNotGrounded();

            CharacterActor.Position -=
                CustomUtilities.Multiply(
                    CharacterActor.Up,
                    CharacterConstants.ColliderMinBottomOffset + verticalMovementParameters.jumpDownDistance +
                    groundDisplacementExtraDistance
                );

            CharacterActor.VerticalVelocity -= CustomUtilities.Multiply(
                CharacterActor.Up, verticalMovementParameters.jumpDownVerticalVelocity);
        }

        #endregion

        #region Jump

        void ResetJump()
        {
            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;
            groundedJumpAvailable = true;
        }

        protected virtual void ProcessRegularJump(float dt)
        {
            if (CharacterActor.IsGrounded)
                if (verticalMovementParameters.canJumpOnUnstableGround || CharacterActor.IsStable)
                    ResetJump();

            if (isAllowedToCancelJump)
            {
                if (verticalMovementParameters.cancelJumpOnRelease)
                {
                    if (CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMaxTime ||
                        CharacterActor.IsFalling)
                    {
                        isAllowedToCancelJump = false;
                    }
                    else if (!CharacterActions.jump.value && CharacterActions.jump.StartedElapsedTime >=
                             verticalMovementParameters.cancelJumpMinTime)
                    {
                        // Get the velocity mapped onto the current jump direction
                        var projectedJumpVelocity = Vector3.Project(CharacterActor.Velocity, jumpDirection);

                        CharacterActor.Velocity -= CustomUtilities.Multiply(
                            projectedJumpVelocity, 1f - verticalMovementParameters.cancelJumpMultiplier);

                        isAllowedToCancelJump = false;
                    }
                }
            }
            else
            {
                var jumpResult = CanJump();

                switch (jumpResult)
                {
                    case JumpResult.Grounded:
                        groundedJumpAvailable = false;

                        break;
                    case JumpResult.NotGrounded:
                        notGroundedJumpsLeft--;

                        return; // Prevents any further jump processing


                    case JumpResult.Invalid:
                        return;
                }

                // Events ---------------------------------------------------
                if (CharacterActor.IsGrounded)
                    OnGroundedJumpPerformed?.Invoke(true);
                else
                    OnNotGroundedJumpPerformed?.Invoke(notGroundedJumpsLeft);

                OnJumpPerformed?.Invoke();

                var agilityJumpMultiplier = 1 + (attributesManager.Agility - 1) * 0.1f;

                jumpStartFeedbacks?.PlayFeedbacks();

                // Define the jump direction ---------------------------------------------------
                jumpDirection = SetJumpDirection();

                // Force "not grounded" state.     
                if (CharacterActor.IsGrounded)
                    CharacterActor.ForceNotGrounded();

                // First remove any velocity associated with the jump direction.
                CharacterActor.Velocity -= Vector3.Project(CharacterActor.Velocity, jumpDirection);
                CharacterActor.Velocity += CustomUtilities.Multiply(
                    jumpDirection, verticalMovementParameters.jumpSpeed * agilityJumpMultiplier);


                if (verticalMovementParameters.cancelJumpOnRelease)
                    isAllowedToCancelJump = true;
            }
        }

        /// <summary>
        ///     Returns the jump direction vector whenever the jump action is started.
        /// </summary>
        protected virtual Vector3 SetJumpDirection()
        {
            return CharacterActor.Up;
        }

        #endregion
    }
}
