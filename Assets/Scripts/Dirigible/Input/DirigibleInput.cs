using Dirigible.Controllers;
using Dirigible.Interactable;
using Interfaces;
using Manager.Global;
using Rewired;
using Rewired.Integration.Cinemachine3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dirigible.Input
{
    [RequireComponent(typeof(DirigibleMovementController))]
    public class DirigibleInput : MonoBehaviour, IPlayerInput
    {
        public enum InputActions
        {
            Interact,
            ChangeEffect,
            ApplyEffect,
            Inventory
        }

        // Rewired action indices (Action IDs)
        const int TurnActionId = 35;
        const int ThrustActionId = 34;

        const int ChangeHeightActionId = 36;

        // There is no 3
        const int UseChosenAbilityActionId = 93;
        const int ScrollChosenAbilitiesActionId = 94;
        const int LookYActionId = 6;
        const int LookXActionId = 7;
        const int ZoomActionId = 8;
        const int InteractDirigibleActionId = 47;
        const int InventoryActionId = 27;
        const int ToggleLightsActionId = 95;
        public int airshipPlayerId;

        // public DirigibleCameraController dirigibleCameraController;

        [SerializeField] RewiredCinemachineInputAxisController rewiredCinemachineInputAxisController;

        [FormerlySerializedAs("dirigibleEffectController")]
        public DirigibleAbilityController dirigibleAbilityController;

        public DirigibleInteraction dirigibleInteraction;

        public Player airshipPlayer;
        DirigibleMovementController dirigibleMovementController;

        PauseManager pauseManager;
        public static DirigibleInput Instance { get; private set; }


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            airshipPlayer = ReInput.players.GetPlayer(airshipPlayerId);
            dirigibleMovementController = gameObject.GetComponent<DirigibleMovementController>();
            pauseManager = PauseManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (pauseManager.IsPaused()) return;
            GetInput();
        }

        // Get All Input Values from Rewired Actions
        void GetInput()
        {
            dirigibleMovementController.turnValue = airshipPlayer.GetAxis(TurnActionId);
            dirigibleMovementController.thrustValue = airshipPlayer.GetAxis(ThrustActionId);
            dirigibleMovementController.changeHeightValue = airshipPlayer.GetAxis(ChangeHeightActionId);

            dirigibleAbilityController.applyAbility = airshipPlayer.GetButtonDown(UseChosenAbilityActionId);
            dirigibleAbilityController.changeAbility = airshipPlayer.GetAxis(ScrollChosenAbilitiesActionId);

            if (airshipPlayer.GetButtonDown(ToggleLightsActionId)) dirigibleAbilityController.ToggleLights();

            // Handle interaction directly instead of setting a flag
            if (airshipPlayer.GetButtonDown(InteractDirigibleActionId)) dirigibleInteraction.TriggerInteraction();
            // dirigibleCameraController.lookYValue = airshipPlayer.GetAxis(LookYActionId);
            // dirigibleCameraController.lookXValue = airshipPlayer.GetAxis(LookXActionId);
            // dirigibleCameraController.zoomValue = airshipPlayer.GetAxis(ZoomActionId);
        }

        public bool GetButtonInput(InputActions pause)
        {
            switch (pause)
            {
                case InputActions.Interact:
                    return dirigibleInteraction.interact;
                default:
                    Debug.LogWarning("DirigibleInput: Unhandled input action: " + pause);
                    return false;
            }
        }
    }
}
