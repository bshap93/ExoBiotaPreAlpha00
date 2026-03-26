using System.Collections;
using Dirigible.Controllers;
using UnityEngine;

namespace Dirigible.Camera
{
    public class DirigibleCameraController : MonoBehaviour
    {
        [Header("Input Values (Set by DirigibleInput)")]
        public float lookYValue;

        public float lookXValue;
        public float zoomValue;

        [Header("Target")] [Tooltip("The dirigible transform to follow")] [SerializeField]
        Transform targetTransform;

        [SerializeField] Vector3 offsetFromTarget = new(0, 2f, 0); // Offset from dirigible center

        [Header("Camera Movement")] [SerializeField]
        float yawSpeed = 120f;

        [SerializeField] float pitchSpeed = 90f;

        [Range(-85f, 85f)] [SerializeField] float maxPitchAngle = 75f;

        [Range(-85f, 85f)] [SerializeField]
        float minPitchAngle = -45f; // Allow looking down more than up for aerial view

        [SerializeField] float initialPitch = 15f; // Start slightly looking down

        [Header("Zoom")] [SerializeField] float distanceToTarget = 15f; // Start further back for dirigible

        [SerializeField] float zoomSpeed = 40f;
        [SerializeField] float zoomLerpSpeed = 5f;
        [SerializeField] float minZoom = 5f;
        [SerializeField] float maxZoom = 50f; // Allow zooming out far for aerial view

        [Header("Follow Behavior")]
        // [SerializeField]
        // float followSpeed = 2f;

        // [SerializeField] float rotationFollowSpeed = 1f;
        [SerializeField]
        bool followTargetRotation = true;
        [SerializeField] float followRotationDelay = 0.75f; // Slight delay for smoother experience

        [Header("Collision Detection")] [SerializeField]
        bool enableCollisionDetection = true;

        [SerializeField] float collisionRadius = 1f;
        [SerializeField] LayerMask collisionLayers = -1;

        [Header("Input Settings")] [SerializeField]
        float mouseSensitivity = 1f;

        [SerializeField] bool invertYAxis;

        [Header("Smoothing Settings")] [SerializeField]
        float positionSmoothTime = 0.25f;

        [SerializeField] float rotationSmoothTime = 0.5f;

        readonly RaycastHit[] hitBuffer = new RaycastHit[10];

        // Private variables
        UnityEngine.Camera cameraComponent;
        Transform cameraTransform;
        float currentDistance;
        float currentPitch;

        Vector3 currentTargetPosition;

        float currentYaw;
        float rotationFollowTimer;
        float targetDistance;
        Quaternion targetRotationOffset;
        Vector3 velocityRef;
        float yawVelocity; // <-- add this near your other private variables

        void Awake()
        {
            cameraComponent = GetComponent<UnityEngine.Camera>();
            if (cameraComponent == null) cameraComponent = gameObject.AddComponent<UnityEngine.Camera>();
            cameraTransform = transform;

            // If no target specified, try to find dirigible in parent
            if (targetTransform == null)
            {
                var dirigible = GetComponentInParent<DirigibleMovementController>();
                if (dirigible != null)
                    targetTransform = dirigible.transform;
            }
        }

        void Start()
        {
            if (targetTransform == null)
            {
                Debug.LogError("DirigibleCameraController: No target transform assigned!");
                enabled = false;
                return;
            }

            // Initialize camera position and rotation
            currentTargetPosition = targetTransform.position + targetTransform.TransformDirection(offsetFromTarget);
            currentYaw = targetTransform.eulerAngles.y;
            currentPitch = initialPitch;
            currentDistance = targetDistance = distanceToTarget;

            UpdateCameraPosition();
        }

        void LateUpdate()
        {
            if (targetTransform == null) return;

            // Check if game is paused
            if (Time.timeScale == 0) return;

            HandleInput();
            UpdateTargetFollow();
            UpdateCameraPosition();
        }

        void OnDrawGizmosSelected()
        {
            if (targetTransform == null) return;

            // Draw target position
            Gizmos.color = Color.green;
            var targetPos = targetTransform.position + targetTransform.TransformDirection(offsetFromTarget);
            Gizmos.DrawWireSphere(targetPos, 0.5f);

            // Draw camera collision sphere
            if (enableCollisionDetection)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, collisionRadius);
            }

            // Draw camera to target line
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetPos);
        }

        void OnValidate()
        {
            // Clamp values in inspector
            initialPitch = Mathf.Clamp(initialPitch, minPitchAngle, maxPitchAngle);
            distanceToTarget = Mathf.Clamp(distanceToTarget, minZoom, maxZoom);
        }

        void HandleInput()
        {
            var deltaTime = Time.deltaTime;

            // Apply mouse sensitivity and Y-axis inversion
            var mouseX = lookXValue * mouseSensitivity;
            var mouseY = lookYValue * mouseSensitivity;

            if (invertYAxis)
                mouseY = -mouseY;

            // Update yaw (horizontal rotation)
            currentYaw += mouseX * yawSpeed * deltaTime;

            // Update pitch (vertical rotation) with clamping
            currentPitch -= mouseY * pitchSpeed * deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minPitchAngle, maxPitchAngle);

            // Update zoom
            if (Mathf.Abs(zoomValue) > 0.01f)
            {
                targetDistance += zoomValue * zoomSpeed * deltaTime;
                targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
            }

            // Smooth zoom transition
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomLerpSpeed * deltaTime);
        }

        void UpdateTargetFollow()
        {
            var targetPos = targetTransform.position + targetTransform.TransformDirection(offsetFromTarget);
            currentTargetPosition = Vector3.SmoothDamp(
                currentTargetPosition, targetPos, ref velocityRef, positionSmoothTime);

            if (followTargetRotation)
            {
                var isUserLooking = Mathf.Abs(lookXValue) > 0.01f || Mathf.Abs(lookYValue) > 0.01f;

                // Check if dirigible is turning
                var dirigible = targetTransform.GetComponent<DirigibleMovementController>();
                var dirigibleIsTurning = dirigible != null && Mathf.Abs(dirigible.turnValue) > 0.1f; // Higher threshold

                // Simple: if turning OR looking, don't follow. Otherwise, follow slowly.
                if (!isUserLooking && !dirigibleIsTurning)
                {
                    rotationFollowTimer += Time.deltaTime;
                    if (rotationFollowTimer >= followRotationDelay)
                    {
                        var targetYaw = targetTransform.eulerAngles.y;
                        // currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * 0.5f); // Very slow
                        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationSmoothTime);
                    }
                }
                else
                {
                    rotationFollowTimer = 0f;
                }
            }
        }


        // private void UpdateTargetFollow()
        // {
        //     Vector3 targetPos = targetTransform.position + targetTransform.TransformDirection(offsetFromTarget);
        //     currentTargetPosition = Vector3.SmoothDamp(
        //         currentTargetPosition, targetPos, ref velocityRef, positionSmoothTime);
        //
        //     if (followTargetRotation)
        //     {
        //         bool isUserLooking = Mathf.Abs(lookXValue) > 0.01f || Mathf.Abs(lookYValue) > 0.01f;
        //
        //         // Check if dirigible is turning - get the dirigible's input
        //         DirigibleMovementController dirigible = targetTransform.GetComponent<DirigibleMovementController>();
        //         bool dirigibleIsTurning = dirigible != null && Mathf.Abs(dirigible.turnValue) > 0.05f;
        //
        //         if (isUserLooking || dirigibleIsTurning)
        //         {
        //             rotationFollowTimer = 0f; // Reset timer when user is looking OR dirigible is turning
        //             // STOP any ongoing camera rotation by setting velocity to zero
        //             yawVelocity = 0f;
        //         }
        //         else
        //         {
        //             // Only follow when dirigible is NOT turning and user is NOT looking
        //             rotationFollowTimer += Time.deltaTime;
        //             if (rotationFollowTimer >= followRotationDelay)
        //             {
        //                 float targetYaw = targetTransform.eulerAngles.y;
        //         
        //                 // Gentle following when dirigible is going straight
        //                 currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationSmoothTime * 2f);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         rotationFollowTimer = 0f;
        //         yawVelocity = 0f; // Also stop when following is disabled
        //     }
        // }

        void UpdateCameraPosition()
        {
            // Calculate the rotation based on current yaw and pitch
            var rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

            // Calculate the desired camera position
            var direction = rotation * Vector3.back;
            var desiredPosition = currentTargetPosition + direction * currentDistance;

            // Handle collision detection
            if (enableCollisionDetection) desiredPosition = HandleCollision(currentTargetPosition, desiredPosition);

            // Apply the final position and rotation
            cameraTransform.position = desiredPosition;
            cameraTransform.LookAt(currentTargetPosition);
        }

        Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            var direction = (desiredPos - targetPos).normalized;
            var distance = Vector3.Distance(targetPos, desiredPos);

            // Perform sphere cast to detect collisions
            var hitCount = Physics.SphereCastNonAlloc(
                targetPos,
                collisionRadius,
                direction,
                hitBuffer,
                distance,
                collisionLayers,
                QueryTriggerInteraction.Ignore
            );

            if (hitCount > 0)
            {
                var closestDistance = distance;

                // Find the closest valid hit
                for (var i = 0; i < hitCount; i++)
                {
                    var hit = hitBuffer[i];

                    // Skip if hit distance is too close to target
                    if (hit.distance < 1f) continue;

                    if (hit.distance < closestDistance) closestDistance = hit.distance - collisionRadius;
                }

                // Adjust position to avoid collision
                if (closestDistance < distance)
                {
                    closestDistance = Mathf.Max(closestDistance, minZoom);
                    return targetPos + direction * closestDistance;
                }
            }

            return desiredPos;
        }

        // Public methods for external control
        public void SetTarget(Transform newTarget)
        {
            targetTransform = newTarget;
            if (targetTransform != null)
                currentTargetPosition = targetTransform.position + targetTransform.TransformDirection(offsetFromTarget);
        }

        public void SetCameraDistance(float distance)
        {
            targetDistance = Mathf.Clamp(distance, minZoom, maxZoom);
        }

        public void ResetCamera()
        {
            if (targetTransform != null)
            {
                currentYaw = targetTransform.eulerAngles.y;
                currentPitch = initialPitch;
                targetDistance = distanceToTarget;
                rotationFollowTimer = 0f;
            }
        }

        public void SetFollowRotation(bool follow)
        {
            followTargetRotation = follow;
            rotationFollowTimer = 0f;
        }

        // Getters for debugging or UI
        public float GetCurrentDistance()
        {
            return currentDistance;
        }

        public float GetCurrentYaw()
        {
            return currentYaw;
        }

        public float GetCurrentPitch()
        {
            return currentPitch;
        }

        public Vector3 GetTargetPosition()
        {
            return currentTargetPosition;
        }

        public void TransitionToTarget(Vector3 targetPosition, Quaternion targetRotation, float duration = 2f)
        {
            StartCoroutine(TransitionCoroutine(targetPosition, targetRotation, duration));
        }


        IEnumerator TransitionCoroutine(Vector3 targetPos, Quaternion targetRot, float duration)
        {
            var startPos = transform.position;
            var startRot = transform.rotation;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Final snap to exact target
            transform.position = targetPos;
            transform.rotation = targetRot;

            // Disable this camera after transition
            enabled = false;
        }
    }
}
