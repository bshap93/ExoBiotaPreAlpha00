using Rewired;
using Unity.Cinemachine;
using UnityEngine;

namespace Dirigible.Camera
{
    public class RewiredAxisReader : IInputAxisReader
    {
        private Player _player;
        [SerializeField] private string actionName = "TurnDirigible"; // will be duplicated per-axis
        [SerializeField] private readonly bool cancelDeltaTime = false;
        [SerializeField] private readonly float gain = 1f;
        [SerializeField] private readonly int playerId = 0;

        public float GetValue(Object context,
            IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            EnsurePlayer();
            var v = _player?.GetAxis(actionName) ?? 0f;
            if (Time.deltaTime > 0 && cancelDeltaTime) v /= Time.deltaTime;
            return v * gain;
        }

        private void EnsurePlayer()
        {
            if (_player == null) _player = ReInput.players.GetPlayer(playerId);
        }

        // Optional: expose setters for inspector convenience
        public void SetAction(string name)
        {
            actionName = name;
        }
    }
}