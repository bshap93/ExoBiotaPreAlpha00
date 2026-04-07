using System.Collections.Generic;
using System.Linq;
using Animancer;
using Dirigible.Input;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.Dialog;
using Helpers.Events.NPCs;
using Helpers.Events.Progression;
using Lightbug.Utilities;
using Manager;
using Manager.DialogueScene;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Overview.NPC;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    [RequireComponent(typeof(AlienNPCAnimancerController))]
    public class HumanoidNPCCreature : EnemyController, MMEventListener<AlienNotifyFriendsOfStateEvent>, IInteractable,
        IHoverable, IBillboardable
    {
        [Header("Weapon Setup")] [SerializeField]
        bool hasWeapon;
        [ShowIf("hasWeapon")] [SerializeField] EnemyWeaponDefinition defaultWeaponDefinition;
        [ShowIf("hasWeapon")] [SerializeField] Transform primaryWeaponAnchor;
        [Header("Humanoid Animation")] [SerializeField]
        AlienNPCAnimancerController animancerController;
        [SerializeField] AlienNPCState[] statesWithFullBodyAnimations;

        [Header("Ambulation Feedbacks")] [SerializeField]
        MMFeedbacks walkingLoopFeedbacks;
        [SerializeField] MMFeedbacks runningLoopFeedbacks;
        [Header("Interaction")] [SerializeField]
        float interactDistanceOverride = 5f;
        [SerializeField] string defaultStartNode;

        [SerializeField] LayerMask npcInteractableLayer;
        [SerializeField] LayerMask enemyLayer;

        [Header("Dialogue Camera")] [SerializeField]
        Transform dialogueFocusPoint;
        [SerializeField] MMFeedbacks startDialogueFeedback;

        [ValueDropdown("GetNpcIdOptions")] public
            string npcId;
        [Header("Controls Help & Action Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;
        [SerializeField] NpcDefinition npcDefinition;

        AnimationClip _equippedHoldPose; // cached at equip time
        Coroutine _footstepCoroutine;
        SceneObjectData _sceneObjectData;
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
        public string GetName()
        {
            return npcDefinition != null ? npcDefinition.characterName : "NPC";
        }
        public Sprite GetIcon()
        {
            return npcDefinition != null ? npcDefinition.characterIcon : null;
        }
        public string ShortBlurb()
        {
            return npcDefinition != null ? npcDefinition.npcDescription : string.Empty;
        }
        public Sprite GetActionIcon()
        {
            // For now, just return a generic talk icon. This can be expanded in the future to return different icons based on the NPC's state or other factors.
            return PlayerUIManager.Instance.defaultIconRepository.talkIcon;
        }
        public string GetActionText()
        {
            // For now, just return "Talk". This can be expanded in the future to return different action texts based on the NPC's state or other factors.
            return "Talk";
        }
        public bool OnHoverStart(GameObject go)
        {
            _sceneObjectData = SceneObjectData.Empty();

            _sceneObjectData.ActionIcon = GetActionIcon();
            _sceneObjectData.ActionText = GetActionText();
            _sceneObjectData.Name = GetName();
            _sceneObjectData.ShortBlurb = ShortBlurb();
            _sceneObjectData.Icon = GetIcon();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Show);

            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (_sceneObjectData == null) _sceneObjectData = SceneObjectData.Empty();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Hide);
            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);

            return true;
        }

        public void Interact()
        {
            if (!CanInteract()) return;


            var nodeToUse = GetAppropriateDialogueNode();

            StartDialogue(nodeToUse);
        }
        public void Interact(string param)
        {
            Interact();
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
            DialogueCameraEvent.Trigger(DialogueCameraEventType.ReleaseFocus);
        }
        public bool CanInteract()
        {
            return IsInteractable();
        }
        public bool IsInteractable()
        {
            if (isDead) return false;
            if (IsHostile) return false;
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }

        public float GetInteractionDistance()
        {
            return interactDistanceOverride;
        }
        public void OnMMEvent(AlienNotifyFriendsOfStateEvent eventType)
        {
            if (uniqueIdOfFriends.Contains(eventType.UniqueID) && eventType.IsHostile) SetState(CurrentState, true);
        }
        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        void StartDialogue(string nodeToUse)
        {
            if (nodeToUse.IsNullOrWhiteSpace())
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, nodeToUse);

            var friendlyNPCManager = FriendlyNPCManager.Instance;
            if (friendlyNPCManager != null && !friendlyNPCManager.HasNPCBeenContactedAtLeastOnce(npcDefinition.npcId))
                EnemyXPRewardEvent.Trigger(npcDefinition.xpForFirstMeeting);

            // Focus the dialogue camera on this NPC
            var focusTarget = dialogueFocusPoint != null ? dialogueFocusPoint : transform;
            DialogueCameraEvent.Trigger(DialogueCameraEventType.FocusOnTarget, focusTarget);

            startDialogueFeedback?.PlayFeedbacks();
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }

        protected string GetAppropriateDialogueNode()
        {
            // For now, just return the default start node.
            return defaultStartNode;
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

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

        void OnDrawGizmosSelected()
        {
            var target = dialogueFocusPoint != null ? dialogueFocusPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.08f);
            Gizmos.DrawLine(transform.position, target);
        }

#endif
    }
}
