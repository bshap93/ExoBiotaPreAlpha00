using Dirigible.Controllers;
using UnityEngine;

namespace Domains.Gameplay.DirigibleFlight
{
    public class RotorHousingTilt : MonoBehaviour
    {
        public enum RotorSide
        {
            Left,
            Right
        }

        [SerializeField] private RotorSide rotorSide;

        [Header("Tilt Settings")] [SerializeField]
        private float maxTiltAngle = 30f;

        [SerializeField] private float tiltSmoothTime = 0.2f;

        public float currentTiltAngle;
        public float tiltVelocity;

        private Quaternion baseLocalRotation;

        private void Awake()
        {
            baseLocalRotation = transform.localRotation;
        }

        public void UpdateTilt(DirigibleStatus status)
        {
            // 1. Tilt for forward/back motion
            var backwardThrust = Mathf.Clamp(-status.currentThrust, 0f, 1f);
            var forwardBackTilt = backwardThrust * maxTiltAngle;

            //
            // var backwardThrust = Mathf.Clamp(-status.currentThrust, 0f, 1f);
            // var targetTilt = backwardThrust * maxTiltAngle;

            // 2. Tilt for turning
            var turnInput = status.currentTurnInput; // You’d add this to DirigibleStatus if needed
            var turnTilt = 0f;


            if (rotorSide == RotorSide.Left)
                turnTilt = turnInput * maxTiltAngle; // Left rotors tilt with turn input
            else if (rotorSide == RotorSide.Right)
                turnTilt = -turnInput * maxTiltAngle; // Right rotors opposite

            // 3. Combine them
            var targetTilt = forwardBackTilt + turnTilt;

            currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTilt, ref tiltVelocity, tiltSmoothTime);

            transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, -currentTiltAngle);

            // currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTilt, ref tiltVelocity, tiltSmoothTime);
            //
            // transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, -currentTiltAngle);
        }
    }
}