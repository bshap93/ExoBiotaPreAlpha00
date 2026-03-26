using UnityEngine;

namespace FirstPersonPlayer.Tools
{
    /// <summary>
    ///     Prevents the first-person hand/tool from clipping through walls
    ///     by detecting proximity and adjusting position or visibility
    /// </summary>
    public class HandWallCollisionPrevention : MonoBehaviour
    {
        public enum ResponseMode
        {
            PullBack, // Move hand backwards
            Hide, // Fade out/hide hand
            HideInstant // Instant hide (no fade)
        }

        [Header("Detection")] [SerializeField] Transform cameraTransform;
        [SerializeField] float detectionDistance = 0.5f;
        [SerializeField] LayerMask wallLayers = ~0;

        [Header("Response Type")] [SerializeField]
        ResponseMode responseMode = ResponseMode.PullBack;

        [Header("Pull Back Settings")] [SerializeField]
        Transform handTransform; // The hand/arm root
        [SerializeField] float pullBackAmount = 0.3f;
        [SerializeField] float pullBackSpeed = 10f;

        [Header("Hide Settings")] [SerializeField]
        GameObject[] objectsToHide; // Hand objects to hide
        [SerializeField] float fadeSpeed = 15f;
        float _currentAlpha = 1f;
        float _currentPullBackAmount;
        Renderer[] _handRenderers;

        Vector3 _originalHandPosition;

        void Start()
        {
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;

            if (handTransform != null)
                _originalHandPosition = handTransform.localPosition;

            // Get all renderers if using hide mode
            if (responseMode == ResponseMode.Hide) _handRenderers = GetComponentsInChildren<Renderer>();
        }

        void Update()
        {
            if (cameraTransform == null) return;

            // Raycast forward to detect walls
            var wallNearby = Physics.Raycast(
                cameraTransform.position,
                cameraTransform.forward,
                out var hit,
                detectionDistance,
                wallLayers,
                QueryTriggerInteraction.Ignore
            );

            switch (responseMode)
            {
                case ResponseMode.PullBack:
                    HandlePullBack(wallNearby, hit);
                    break;

                case ResponseMode.Hide:
                    HandleFade(wallNearby);
                    break;

                case ResponseMode.HideInstant:
                    HandleInstantHide(wallNearby);
                    break;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (cameraTransform == null) return;

            // Draw detection ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * detectionDistance);
        }

        void HandlePullBack(bool wallNearby, RaycastHit hit)
        {
            if (handTransform == null) return;

            // Calculate target pull back amount based on proximity
            var targetPullBack = 0f;
            if (wallNearby)
            {
                // Closer to wall = more pull back
                var proximity = 1f - hit.distance / detectionDistance;
                targetPullBack = proximity * pullBackAmount;
            }

            // Smoothly interpolate
            _currentPullBackAmount = Mathf.Lerp(
                _currentPullBackAmount,
                targetPullBack,
                Time.deltaTime * pullBackSpeed
            );

            // Apply pull back (move hand backwards along camera's local Z axis)
            handTransform.localPosition = _originalHandPosition +
                                          Vector3.back * _currentPullBackAmount;
        }

        void HandleFade(bool wallNearby)
        {
            // Target alpha: 0 if wall nearby, 1 if clear
            var targetAlpha = wallNearby ? 0f : 1f;

            // Smoothly interpolate
            _currentAlpha = Mathf.Lerp(
                _currentAlpha,
                targetAlpha,
                Time.deltaTime * fadeSpeed
            );

            // Apply to all renderers
            if (_handRenderers != null)
                foreach (var renderer in _handRenderers)
                {
                    if (renderer == null) continue;

                    foreach (var mat in renderer.materials)
                    {
                        // This requires materials to support transparency
                        var color = mat.color;
                        color.a = _currentAlpha;
                        mat.color = color;
                    }
                }
        }

        void HandleInstantHide(bool wallNearby)
        {
            // Simply enable/disable objects
            if (objectsToHide != null)
                foreach (var obj in objectsToHide)
                    if (obj != null)
                        obj.SetActive(!wallNearby);
        }
    }
}
