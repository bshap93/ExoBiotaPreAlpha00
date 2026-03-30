using System.Linq;
using Animancer;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events.Combat;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    [RequireComponent(typeof(AlienNPCAnimancerController))]
    public class HumanoidNPCCreature : EnemyController, MMEventListener<AlienNotifyFriendsOfStateEvent>
    {
        [SerializeField] bool hasWeapon;
        [ShowIf("hasWeapon")] [SerializeField] EnemyWeaponDefinition defaultWeaponDefinition;
        [ShowIf("hasWeapon")] [SerializeField] Transform primaryWeaponAnchor;
        [SerializeField] AlienNPCAnimancerController animancerController;
        [SerializeField] AlienNPCState[] statesWithFullBodyAnimations;

        [SerializeField] MMFeedbacks walkingLoopFeedbacks;
        [SerializeField] MMFeedbacks runningLoopFeedbacks;

        AnimationClip _equippedHoldPose; // cached at equip time
        Coroutine _footstepCoroutine;
        // public AlienNPCState initialDefaultState = AlienNPCState.Working;

        protected AnimancerState WorkingState;

        public GameObject CurrentWeaponInstance { get; private set; }


        AlienNPCState CurrentState { get; set; }

        public EnemyWeaponDefinition EquippedWeapon { get; private set; }

        protected override void Start()
        {
            base.Start();
            if (hasWeapon) EquipWeapon(defaultWeaponDefinition);

            IsHostile = isInitiallyHostile;


            SetState(animancerController.CurrentState, IsHostile);
        }


        protected override void Update()
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

                walkingLoopFeedbacks?.StopFeedbacks();
                runningLoopFeedbacks?.StopFeedbacks();
            }
            else if (speed < walkRunThreshold)
            {
                PlayMovementAnimation(velocity, speed);
                IsPlayingCustomAnimation = false;

                if (walkingLoopFeedbacks != null && !walkingLoopFeedbacks.IsPlaying)
                    walkingLoopFeedbacks.PlayFeedbacks();
            }
            else
            {
                PlayMovementAnimation(velocity, speed);
                IsPlayingCustomAnimation = false;

                if (runningLoopFeedbacks != null && !runningLoopFeedbacks.IsPlaying)
                    runningLoopFeedbacks.PlayFeedbacks();
            }


            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                DeathState = animancerComponent.Play(creatureType.animationSet.deathAnimation, 0.1f);
                DeathState.Events(this).OnEnd = () => { Destroy(gameObject); };

                OnDeath();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<PlayerStartsAttackEvent>();
            this.MMEventStartListening<AlienNotifyFriendsOfStateEvent>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<PlayerStartsAttackEvent>();
            this.MMEventStopListening<AlienNotifyFriendsOfStateEvent>();
        }
        public void OnMMEvent(AlienNotifyFriendsOfStateEvent eventType)
        {
            if (uniqueIdOfFriends.Contains(eventType.UniqueID) && eventType.IsHostile) SetState(CurrentState, true);
        }


        /// <summary>
        ///     Central state setter — call this from NodeCanvas FSM state entry
        ///     actions when you're ready to wire those up.
        /// </summary>
        public void SetState(AlienNPCState newState, bool isHostile)
        {
            var stateChanged = newState != CurrentState || isHostile != IsHostile;

            CurrentState = newState;
            IsHostile = isHostile;

            // Working/stationary states are "custom" from EnemyController's perspective —
            // this prevents Update() from stomping them with IdleState.
            IsPlayingCustomAnimation = newState == AlienNPCState.Working
                                       || newState == AlienNPCState.InDialogue
                                       || newState == AlienNPCState.FriendlyAndHailable;


            if (stateChanged)
                AlienNotifyFriendsOfStateEvent.Trigger(uniqueID, isHostile, newState);


            if (statesWithFullBodyAnimations.Contains(newState))
                animancerController.ClearUpperBody();
            else if (hasWeapon && _equippedHoldPose != null)
                animancerController.PlayWeaponHoldPose(_equippedHoldPose);

            animancerController.PlayAnimationsForState(newState);
        }

        void EquipWeapon(EnemyWeaponDefinition weaponDefinition)
        {
            if (weaponDefinition.enemyWeaponPrefab == null)
            {
                Debug.LogWarning(
                    $"Trying to equip weapon for {name} but the enemyWeaponPrefab field of the provided weaponDefinition is null.");

                return;
            }

            EquippedWeapon = weaponDefinition;

            CurrentWeaponInstance = Instantiate(
                weaponDefinition.enemyWeaponPrefab, primaryWeaponAnchor.position,
                primaryWeaponAnchor.rotation, primaryWeaponAnchor);

            if (weaponDefinition.HoldPoseClip != null)
                animancerController.PlayWeaponHoldPose(weaponDefinition.HoldPoseClip);

            _equippedHoldPose = weaponDefinition.HoldPoseClip;
        }
    }
}
