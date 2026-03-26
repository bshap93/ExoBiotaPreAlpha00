using UnityEngine;

namespace FirstPersonPlayer.FPNPCs
{
    [DisallowMultipleComponent]
    public class CrabSenses : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float fieldOfView = 160f; // crabs: wide-ish

        public Transform Target => player;

        private void Reset()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        public bool HasTarget()
        {
            return player != null;
        }

        public bool TargetWithinDetection()
        {
            return HasTarget() && Vector3.Distance(transform.position, player.position) <= detectionRange;
        }

        public bool TargetWithinAttack()
        {
            return HasTarget() && Vector3.Distance(transform.position, player.position) <= attackRange;
        }

        public bool CanSeeTarget()
        {
            if (!HasTarget()) return false;
            var to = player.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > detectionRange * detectionRange) return false;

            var fwd = transform.forward;
            var angle = Vector3.Angle(fwd, to);
            if (angle > fieldOfView * 0.5f) return false;

            // Optional: add Physics.Raycast for line of sight if you want occlusion.
            return true;
        }
    }
}