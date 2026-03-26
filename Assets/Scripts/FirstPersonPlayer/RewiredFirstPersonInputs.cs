using Domains.Gameplay.Mining.Events;
using Helpers.Events;
using Helpers.Events.Inventory;
using MoreMountains.Tools;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer
{
    public class RewiredFirstPersonInputs : MonoBehaviour, MMEventListener<PlayerDeathEvent>
    {
        public enum InputActions
        {
            MoveForwardBackward,
            MoveLeftRight,
            Jump,
            Interact,
            UseEquipped,
            Crouch,
            ScrollBetweenTools,
            LookY,
            LookX,
            InteractHeld,
            JumpHeld,
            NoOp,
            SprintOrAbility,
            HeldSprintOrAbility,
            SprintStart,
            SprintStop,
            DropPropOrHold,
            Pause,
            ItemUseModifierHeld,
            ItemUseModifierDown,
            PickablePick,
            HotbarFP1,
            HotbarFP2,
            HotbarFP3,
            HotbarFP4,
            HotbarFP5,
            HotbarFP6
        }

        [Header("Character Input Values")] public Vector2 move;

        [SerializeField] float dropPropOrHoldHeldDuration = 1f;


        public Vector2 look;
        public float scrollBetweenTools;
        public bool jump;
        public bool jumpHeld;
        // Sprint
        // public bool sprint;
        // public bool sprintStart;
        // public bool sprintStop;
        //

        public bool analogMovement;
        public bool interact;
        public bool interactHeld;
        public bool crouch;
        [FormerlySerializedAs("sprintOrAbility")]
        public bool pressedSprintOrAbility;
        public bool heldSprintOrAbility;
        public bool useEquipped;
        public bool heldEquipped;
        [FormerlySerializedAs("pickUpProp")] public bool dropPropOrHold;
        public bool dropPropOrHoldDown;
        public bool pause;
        public bool itemUseModifierDown;
        public bool itemUseModifierUp;
        [FormerlySerializedAs("leftHandToggle")]
        public bool itemUseModifierHeld;
        public bool pickablePick;

        // 1-2 are consumables
        public bool hotbarFP1;
        public bool hotbarFP2;

        //Tool hotbar
        // 3 is empty hand
        public bool hotbarFP3;
        // 4-6 are tools
        public bool hotbarFP4;
        public bool hotbarFP5;
        public bool hotbarFP6;

        float _currentHoldTimeDropPropOrHold;
        bool _isHoldingDropPropOrHold;

        bool _isPlayerDead;

        Player _rewiredPlayer;

        void Start()
        {
            _rewiredPlayer = ReInput.players.GetPlayer(0);
        }

        void Update()
        {
            if (_rewiredPlayer == null || _isPlayerDead) return;

            // Read movement input
            move = new Vector2(
                _rewiredPlayer.GetAxis("Move Horizontal"),
                _rewiredPlayer.GetAxis("Move Vertical")
            );

            Debug.DrawRay(
                transform.position,
                transform.forward * move.y + transform.right * move.x,
                Color.cyan, 0.01f, false);

            // Read look input
            look = new Vector2(
                _rewiredPlayer.GetAxis("Look X"),
                _rewiredPlayer.GetAxis("Look Y")
            );

            // Read button inputs
            jump = _rewiredPlayer.GetButton("Jump");
            interact = _rewiredPlayer.GetButtonDown("Interact");
            interactHeld = _rewiredPlayer.GetButton("Interact");
            crouch = _rewiredPlayer.GetButton("Crouch");

            pressedSprintOrAbility = _rewiredPlayer.GetButtonDown("SprintOrAbility");
            heldSprintOrAbility = _rewiredPlayer.GetButton("SprintOrAbility");

            dropPropOrHold = _rewiredPlayer.GetButton("DropPropOrHold");
            dropPropOrHoldDown = _rewiredPlayer.GetButtonDown("DropPropOrHold");
            useEquipped = _rewiredPlayer.GetButtonDown("UseEquipped");
            heldEquipped = _rewiredPlayer.GetButton("UseEquipped");
            itemUseModifierHeld = _rewiredPlayer.GetButton("ItemUseModifier");
            itemUseModifierDown = _rewiredPlayer.GetButtonDown("ItemUseModifier");
            itemUseModifierUp = _rewiredPlayer.GetButtonUp("ItemUseModifier");
            pickablePick = _rewiredPlayer.GetButtonDown("PickablePick");
            scrollBetweenTools = _rewiredPlayer.GetAxisDelta("ScrollTools");

            hotbarFP1 = _rewiredPlayer.GetButtonDown("HotbarFP1");
            hotbarFP2 = _rewiredPlayer.GetButtonDown("HotbarFP2");
            hotbarFP3 = _rewiredPlayer.GetButtonDown("HotbarFP3");
            hotbarFP4 = _rewiredPlayer.GetButtonDown("HotbarFP4");
            hotbarFP5 = _rewiredPlayer.GetButtonDown("HotbarFP5");
            hotbarFP6 = _rewiredPlayer.GetButtonDown("HotbarFP6");

            if (dropPropOrHold)
            {
                if (!_isHoldingDropPropOrHold)
                {
                    _isHoldingDropPropOrHold = true;
                    _currentHoldTimeDropPropOrHold = 0f;

                    ToolEvent.Trigger(ToolEventType.ToggleToolMode);
                }

                _currentHoldTimeDropPropOrHold += Time.deltaTime;

                if (_currentHoldTimeDropPropOrHold >= dropPropOrHoldHeldDuration)
                    // Held long enough to count as a "hold"
                    // You can trigger any events or actions for a hold here
                    GlobalInventoryEvent.Trigger(
                        GlobalInventoryEventType.UnequipRightHandTool);
            }
            else
            {
                if (_isHoldingDropPropOrHold)
                    ResetHoldDropPropOrHold();

                _isHoldingDropPropOrHold = false;
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(PlayerDeathEvent eventType)
        {
            _isPlayerDead = true;
        }

        void ResetHoldDropPropOrHold()
        {
            _currentHoldTimeDropPropOrHold = 0f;
        }


        public bool GetButtonInput(InputActions input)
        {
            switch (input)
            {
                case InputActions.Jump:
                    return jump;
                case InputActions.Interact:
                    return interact;
                case InputActions.UseEquipped:
                    return useEquipped;

                case InputActions.Crouch:
                    return crouch;

                case InputActions.SprintOrAbility:
                    return pressedSprintOrAbility;

                case InputActions.HeldSprintOrAbility:
                    return heldSprintOrAbility;


                case InputActions.InteractHeld:
                    return interactHeld;
                case InputActions.JumpHeld:
                    return jumpHeld;
                case InputActions.DropPropOrHold:
                    return dropPropOrHold;


                case InputActions.Pause:
                    return pause;
                case InputActions.ItemUseModifierHeld:
                    return itemUseModifierHeld;
                case InputActions.ItemUseModifierDown:
                    return itemUseModifierDown;
                case InputActions.PickablePick:
                    return pickablePick;

                case InputActions.HotbarFP1:
                    return hotbarFP1;
                case InputActions.HotbarFP2:
                    return hotbarFP2;
                case InputActions.HotbarFP3:
                    return hotbarFP3;
                case InputActions.HotbarFP4:
                    return hotbarFP4;
                case InputActions.HotbarFP5:
                    return hotbarFP5;
                case InputActions.HotbarFP6:
                    return hotbarFP6;


                default:
                    return false;
            }
        }

        public float GetAxisInput(InputActions action)
        {
            switch (action)
            {
                case InputActions.MoveForwardBackward:
                    return move.y;
                case InputActions.MoveLeftRight:
                    return move.x;
                case InputActions.LookY:
                    return look.y;
                case InputActions.LookX:
                    return look.x;
                case InputActions.ScrollBetweenTools:
                    return scrollBetweenTools;
                default:
                    return 0f; // Default value if action ID is not recognized
            }
        }
    }
}
