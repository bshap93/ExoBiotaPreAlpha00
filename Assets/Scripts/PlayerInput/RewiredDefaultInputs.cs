using Rewired;
using UnityEngine;

namespace PlayerInput
{
    public class RewiredDefaultInputs : MonoBehaviour
    {
        public enum InputActions
        {
            ToggleInGameUI,
            Pause
        }

        public bool toggleInGameUI;
        public bool pause;

        private Player _rewiredPlayer;
        public static RewiredDefaultInputs Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _rewiredPlayer = ReInput.players.GetPlayer(0); // Assuming player 0 is the default player
        }

        private void Update()
        {
            if (_rewiredPlayer == null) return;

            toggleInGameUI = _rewiredPlayer.GetButton("ToggleInGameUI");
            pause = _rewiredPlayer.GetButtonDown("Pause");

            // You can add more input checks here as needed
        }

        public bool GetButtonInput(InputActions input)
        {
            switch (input)
            {
                case InputActions.Pause:
                    return pause;

                case InputActions.ToggleInGameUI:
                    return toggleInGameUI;
                default:
                    return false;
            }
        }
    }
}