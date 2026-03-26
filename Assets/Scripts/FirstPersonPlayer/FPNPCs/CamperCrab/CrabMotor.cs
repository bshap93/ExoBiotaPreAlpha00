using UnityEngine;

namespace FirstPersonPlayer.FPNPCs
{
    [DisallowMultipleComponent]
    public class CrabMotor : MonoBehaviour
    {
        [SerializeField] private float turnSpeed = 180f; // deg/sec
        [SerializeField] private float moveSpeed = 0.5f; // m/sec (0 = stationary)
        [SerializeField] private Transform graphicsRoot; // rotate visuals only if you prefer

        public bool TurnToward(Vector3 worldTarget, float angleTolerance = 5f)
        {
            var to = worldTarget - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.001f) return true;

            var targetYaw = Quaternion.LookRotation(to.normalized, Vector3.up);
            var rotObj = graphicsRoot ? graphicsRoot : transform;
            rotObj.rotation = Quaternion.RotateTowards(rotObj.rotation, targetYaw, turnSpeed * Time.deltaTime);

            var delta = Quaternion.Angle(rotObj.rotation, targetYaw);
            return delta <= angleTolerance;
        }

        public void StepForward(float dt)
        {
            if (moveSpeed <= 0f) return;
            var fwd = graphicsRoot ? graphicsRoot.forward : transform.forward;
            transform.position += fwd * moveSpeed * dt;
        }
    }
}