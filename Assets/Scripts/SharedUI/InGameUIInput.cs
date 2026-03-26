using Rewired;
using UnityEngine;

namespace SharedUI
{
    public class InGameUIInput : MonoBehaviour
    {
        public enum InputActions
        {
            CloseUI
        }

        public bool closeUI;

        private Player _player;

        private void Awake()
        {
            _player = ReInput.players.GetPlayer(0);
        }

        private void Update()
        {
            if (_player == null) return;

            closeUI = _player.GetButtonDown("CloseUI");
        }

        public bool GetButtonInput(InputActions action)
        {
            switch (action)
            {
                case InputActions.CloseUI:
                    return closeUI;
                default:
                    Debug.LogWarning($"Unhandled InputAction: {action}");
                    return false;
            }
        }
    }
}