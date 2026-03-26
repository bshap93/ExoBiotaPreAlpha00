using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks
{
    [Category("AttackMoves")]
    [Description("Fires a projectile from the spitting plant after aiming is complete")]
    public class SpittingVineShootProjectile : ActionTask
    {
        [Tooltip("Upward arc to add to trajectory (0 = direct shot)")] [SliderField(0f, 45f)]
        public BBParameter<float> ArcAngle = 0f;

        public BBParameter<GameObject> MuzzleFlashPrefab;

        [Tooltip("Layer mask for the projectile")]
        public LayerMask ProjectileLayerMask = -1;
        [RequiredField] [Tooltip("The projectile prefab to spawn")]
        public BBParameter<GameObject> ProjectilePrefab;
        // public BBParameter<GameObject> HitBurstEffectPrefab;

        [Tooltip("Projectile speed (units per second)")]
        public BBParameter<float> ProjectileSpeed = 15f;

        [RequiredField] [Tooltip("The target to shoot at")]
        public BBParameter<GameObject> ShootTarget;

        [Tooltip("Optional: Reference to the aiming task to get accurate spawn position")]
        public BBParameter<string> TrunkBonePrefix = "Trunk_";

        [Tooltip("Number of trunk segments to find the flower")]
        public BBParameter<int> TrunkSegmentCount = 8;

        [Tooltip("Vertical aim adjustment in meters (negative to aim lower, positive to aim higher)")]
        [SliderField(-2f, 2f)]
        public BBParameter<float> VerticalAimAdjustment = -0.5f;

        protected override string info => $"Shoot projectile at {ShootTarget}";

        protected override void OnExecute()
        {
            if (ProjectilePrefab.value == null)
            {
                Debug.LogWarning("SpittingVineShootProjectile: ProjectilePrefab is null!");
                EndAction(false);
                return;
            }

            if (ShootTarget.value == null)
            {
                Debug.LogWarning("SpittingVineShootProjectile: ShootTarget is null!");
                EndAction(false);
                return;
            }

            // Get spawn position (flower location)
            var spawnPosition = GetFlowerPosition();

            // Get the actual aim direction from the bent plant
            var shootDirection = GetPlantAimDirection();

            // If we couldn't get the plant's aim direction, calculate manually
            if (shootDirection == Vector3.zero || shootDirection.sqrMagnitude < 0.01f)
            {
                Debug.LogWarning(
                    "SpittingVineShootProjectile: Could not get plant aim direction, calculating from spawn to target");

                shootDirection = CalculateShootDirection(spawnPosition);
            }

            // Spawn muzzle flash if specified
            if (MuzzleFlashPrefab.value != null)
                Object.Instantiate(
                    MuzzleFlashPrefab.value, spawnPosition, Quaternion.LookRotation(shootDirection));

            // Spawn projectile
            var projectile = Object.Instantiate(
                ProjectilePrefab.value, spawnPosition, Quaternion.LookRotation(shootDirection));

            // Configure projectile
            ConfigureProjectile(projectile, shootDirection);

            // Task completes immediately after firing
            EndAction(true);
        }

        Vector3 GetPlantAimDirection()
        {
            // Try to get the actual aim direction from the bent trunk
            var topSegment = FindTopTrunkSegment();

            if (topSegment != null)
            {
                // The plant's local -X direction is where it's aiming
                var aimDir = topSegment.TransformDirection(Vector3.left);

                // Apply vertical adjustment if specified (negative = aim lower)
                if (Mathf.Abs(VerticalAimAdjustment.value) > 0.01f)
                {
                    // Add a vertical component to adjust aim up or down
                    aimDir = aimDir + Vector3.down * VerticalAimAdjustment.value;
                    aimDir.Normalize();
                    Debug.Log(
                        $"Applied vertical adjustment of {VerticalAimAdjustment.value}m, new direction: {aimDir}");
                }

                Debug.Log($"Using plant's actual aim direction: {aimDir}");
                return aimDir;
            }

            return Vector3.zero;
        }

        Vector3 GetFlowerPosition()
        {
            // Try to find the top trunk segment to calculate flower position
            var topSegment = FindTopTrunkSegment();

            if (topSegment != null)
                // Flower is 0.2 units in local -X direction from top segment
                return topSegment.position + topSegment.TransformDirection(Vector3.left * 0.2f);

            // Fallback: use agent position with height offset
            return agent.transform.position + Vector3.up * 1.6f; // Approximate height
        }

        Transform FindTopTrunkSegment()
        {
            // Try to find the last trunk segment
            var possibleNames = new[]
            {
                $"{TrunkBonePrefix.value}{TrunkSegmentCount.value:D2}",
                $"{TrunkBonePrefix.value}{TrunkSegmentCount.value}",
                $"{TrunkBonePrefix.value}{TrunkSegmentCount.value - 1:D2}",
                $"{TrunkBonePrefix.value}{TrunkSegmentCount.value - 1}"
            };

            foreach (var name in possibleNames)
            {
                var found = FindTransformRecursive(agent.transform, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        Transform FindTransformRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            foreach (Transform child in parent)
            {
                var result = FindTransformRecursive(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        Vector3 CalculateShootDirection(Vector3 fromPosition)
        {
            var targetPosition = ShootTarget.value.transform.position;

            // Aim at target's center (approximate)
            var targetCollider = ShootTarget.value.GetComponent<Collider>();
            if (targetCollider != null)
                targetPosition = targetCollider.bounds.center;
            else
                // If no collider, assume character center is about 1m above feet
                targetPosition += Vector3.up * 1.0f;

            // Apply vertical aim adjustment
            targetPosition += Vector3.up * VerticalAimAdjustment.value;

            // Calculate base direction
            var direction = (targetPosition - fromPosition).normalized;

            // Add upward arc if specified (usually keep at 0)
            if (ArcAngle.value > 0f)
            {
                // Rotate direction upward by arc angle
                var horizontal = new Vector3(direction.x, 0f, direction.z).normalized;
                direction = Quaternion.AngleAxis(ArcAngle.value, Vector3.Cross(horizontal, Vector3.up)) * direction;
                direction.Normalize();
            }

            Debug.Log($"Calculated direction to target at {targetPosition}, direction: {direction}");
            return direction;
        }

        void ConfigureProjectile(GameObject projectile, Vector3 direction)
        {
            // Setup Rigidbody if present
            var rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Make sure it's not kinematic
                rb.isKinematic = false;
                rb.useGravity = false;

                // Set velocity (use linearVelocity for Unity 6+, velocity for older versions)
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = direction * ProjectileSpeed.value;
                Debug.Log(
                    $"Rigidbody configured: linearVelocity = {rb.linearVelocity}, magnitude = {rb.linearVelocity.magnitude}");
#else
                rb.velocity = direction * ProjectileSpeed.value;
                Debug.Log($"Rigidbody configured: velocity = {rb.velocity}, magnitude = {rb.velocity.magnitude}");
#endif
            }
            else
            {
                Debug.LogWarning(
                    $"SpittingVineShootProjectile: Projectile prefab '{ProjectilePrefab.value.name}' has no Rigidbody! Add a Rigidbody component for physics-based movement.");
            }

            // Setup projectile script if using a custom component
            var projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null) projectileScript.speed = ProjectileSpeed.value;

            // Set layer if needed
            if (ProjectileLayerMask != -1) projectile.layer = GetLayerFromMask(ProjectileLayerMask);

            Debug.Log(
                $"Projectile fired at {ProjectileSpeed.value} m/s toward {ShootTarget.value.name}, direction: {direction}");
        }

        int GetLayerFromMask(LayerMask mask)
        {
            var layerNumber = 0;
            var layer = mask.value;
            while (layer > 1)
            {
                layer = layer >> 1;
                layerNumber++;
            }

            return layerNumber;
        }
    }

    #region Example Projectile Component

    /// <summary>
    ///     Example projectile component - replace with your actual projectile implementation
    ///     This provides fallback movement if no Rigidbody is present
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public float damage = 10f;
        public float speed = 15f;
        public float lifetime = 5f;

        Rigidbody _rb;
        bool _useManualMovement;

        void Start()
        {
            // Check if we have a Rigidbody
            _rb = GetComponent<Rigidbody>();
            _useManualMovement = _rb == null;

            if (_useManualMovement) Debug.LogWarning("Projectile: No Rigidbody found, using manual Transform movement");

            // Auto-destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            // If no Rigidbody, move using Transform
            if (_useManualMovement) transform.position += transform.forward * speed * Time.deltaTime;
        }

        void OnCollisionEnter(Collision collision)
        {
            // Handle collision with target
            // Apply damage, spawn effects, etc.
            Debug.Log($"Projectile hit: {collision.gameObject.name}");

            Destroy(gameObject);
        }
    }

    #endregion
}
