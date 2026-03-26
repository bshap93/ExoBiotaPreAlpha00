using System;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;
using UnityEngine.Serialization;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace FirstPersonPlayer.InputHandling
{
    public class RewiredCcpInputHandler : InputHandler
    {
        const int MoveForwardBackActionId = 9;
        const int MoveLeftRightActionId = 10;
        public const int FirstPersonPlayerId = 0;
        const int LookXActionId = 44; // Add this
        const int LookYActionId = 45; // Add this

        [FormerlySerializedAs("_fpPlayerInput")] [SerializeField]
        RewiredFirstPersonInputs _rewiredFirstPersonInputs;

        // Dictionary to map Vector2 action names to the two separate axes
        readonly Dictionary<string, Vector2Action> vector2Actions = new()
        {
            { "Movement", new Vector2Action(MoveLeftRightActionId, MoveForwardBackActionId) },
            // Add more Vector2 actions here if needed, like:
            { "Look", new Vector2Action(LookXActionId, LookYActionId) }
        };


        public override bool GetBool(string actionName)
        {
            var output = false;
            try
            {
                switch (actionName)
                {
                    case "Jump":
                        output = _rewiredFirstPersonInputs.GetButtonInput(RewiredFirstPersonInputs.InputActions.Jump);
                        break;
                    case "Run":
                        output = _rewiredFirstPersonInputs.GetButtonInput(
                            RewiredFirstPersonInputs.InputActions.SprintOrAbility);

                        break;
                    case "Interact":
                        output = _rewiredFirstPersonInputs.GetButtonInput(
                            RewiredFirstPersonInputs.InputActions.Interact);

                        break;
                    case "Jet Pack":
                        output = _rewiredFirstPersonInputs.GetButtonInput(
                            RewiredFirstPersonInputs.InputActions.JumpHeld);

                        break;
                    case "Dash":
                        output = _rewiredFirstPersonInputs.GetButtonInput(RewiredFirstPersonInputs.InputActions.NoOp);
                        break;
                    case "Crouch":
                        output = _rewiredFirstPersonInputs.GetButtonInput(RewiredFirstPersonInputs.InputActions.Crouch);
                        break;
                    default:
                        throw new ArgumentException($"Unknown action name: {actionName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error getting bool for action '{actionName}': {e.Message}");
            }

            return output;
        }

        public override float GetFloat(string actionName)
        {
            var output = 0f;
            try
            {
                switch (actionName)
                {
                    case "Pitch":
                        // Camera up/down (mouse Y)
                        output = _rewiredFirstPersonInputs.GetAxisInput(RewiredFirstPersonInputs.InputActions.LookY);
                        break;
                    case "Roll":
                        // Camera roll (usually not used in FPS, leave as 0)
                        output = 0f;
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error getting float for action '{actionName}': {e.Message}");
            }

            return output;
        }

        public override Vector2 GetVector2(string actionName)
        {
            var output = Vector2.zero;

            try
            {
                switch (actionName)
                {
                    case "Movement":
                        var moveX = _rewiredFirstPersonInputs.GetAxisInput(
                            RewiredFirstPersonInputs.InputActions
                                .MoveLeftRight);

                        var moveY = _rewiredFirstPersonInputs.GetAxisInput(
                            RewiredFirstPersonInputs.InputActions
                                .MoveForwardBackward);

                        output = new Vector2(moveX, moveY);
                        break;

                    case "Look":
                        var lookX = _rewiredFirstPersonInputs.GetAxisInput(RewiredFirstPersonInputs.InputActions.LookX);
                        var lookY = _rewiredFirstPersonInputs.GetAxisInput(RewiredFirstPersonInputs.InputActions.LookY);
                        output = new Vector2(lookX, lookY);
                        break;

                    default:
                        Debug.LogWarning($"Unknown Vector2 action name: {actionName}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error getting Vector2 for action '{actionName}': {e.Message}");
            }

            return output;
        }


        struct Vector2Action
        {
            public readonly int xActionId;
            public readonly int yActionId;

            public Vector2Action(int xActionId, int yActionId)
            {
                this.xActionId = xActionId;
                this.yActionId = yActionId;
            }
        }
    }
}
