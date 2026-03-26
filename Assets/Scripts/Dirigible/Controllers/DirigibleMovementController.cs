using System;
using UnityEngine;

namespace Dirigible.Controllers
{
    public class DirigibleMovementController : MonoBehaviour
    {
        [Header("Input Values (Set by DirigibleInput)")]
        public float turnValue;

        public float thrustValue;
        public float changeHeightValue;

        [Header("Movement Settings")] [SerializeField]
        float maxForwardThrust = 1500f;

        [SerializeField] float maxBackwardThrust = 600f; // Less powerful in reverse
        // [SerializeField] float turnTorque = 800f;
        [SerializeField] float stabilizationForce = 100f;

        [Header("Vertical Movement")] [SerializeField]
        float verticalThrustForce = 1200f;

        [SerializeField] float hoverStabilization = 50f;
        [SerializeField] float maxTiltAngle = 15f; // Max forward/back tilt for quadcopter props

        [Header("Physics Settings")] [SerializeField]
        float dragCoefficient = 0.98f; // Atmospheric drag in thin air

        [SerializeField] float angularDragCoefficient = 0.95f;
        [SerializeField] float gravityMultiplier = 0.7f; // Reduced gravity on colony planet
        [SerializeField] float buoyancyForce = 800f; // Gas bag lift

        [Header("Atmospheric Height System")] [SerializeField]
        float maxOperatingHeight = 1000f; // Maximum safe operating height

        [SerializeField] float atmosphericFalloffStart = 800f; // Height where air starts thinning noticeably
        [SerializeField] float criticalHeight = 950f; // Height where performance becomes severely limited

        [SerializeField] AnimationCurve
            airDensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f); // Controls how air density falls off

        [SerializeField] float minAirDensity = 0.05f; // Minimum air density (never goes to zero for stability)
        [SerializeField] float heightWarningThreshold = 850f; // When to start warning effects

        [Header("Atmospheric Effects")] [SerializeField]
        bool enableEngineStrain = true; // Engine works harder in thin air

        // [SerializeField] float strainEffectMultiplier = 2f;
        [SerializeField] bool enableTurbulence = true; // Instability in thin air
        [SerializeField] float turbulenceIntensity = 100f;

        [Header("Responsiveness")] [SerializeField]
        float thrustResponseTime = 0.3f;

        [SerializeField] float turnResponseTime = 0.25f;
        [SerializeField] float verticalResponseTime = 0.4f;

        [Header("Auto-Recovery")] [SerializeField]
        bool autoRightAfterCollision = true;

        [SerializeField] float rightingForce = 2000f;
        public float currentThrust;

        [Header("Altitude Hold (Experimental)")] [SerializeField]
        bool altitudeHoldEnabled;

        [SerializeField] float cruiseAltitudeY = 28f; // world-space Y
        [SerializeField] float holdKp = 30f; // proportional
        [SerializeField] float holdKd = 12f; // damping
        [SerializeField] float maxHoldAccel = 25f; // clamp safety

        // Private variables
        [SerializeField] Rigidbody rb;
        float _baseHeight; // Starting height for relative calculations

        // Atmospheric system variables
        float _currentAirDensity = 1f;

        // Quadcopter tilt simulation
        float _currentQuadTiltAngle;
        float _currentTurnInput;
        float _currentVerticalInput;
        bool _isInDangerZone;
        float _turbulenceTimer;
        float _turnVelocity; // For SmoothDamp
        Vector3 _windForce = Vector3.zero; // For future wind implementation

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Set to a custom layer (e.g., "Dirigible")
            gameObject.layer = LayerMask.NameToLayer("Dirigible");

            // FREEZE Y-axis rotation in physics - we'll handle it manually
            rb.constraints = RigidbodyConstraints.FreezeRotationY;

            // Set up rigidbody for dirigible physics
            rb.mass = 2000f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 2f;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        void Start()
        {
            _baseHeight = transform.position.y;

            // Apply initial buoyancy to counteract some gravity
            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);

            // Initialize air density curve if not set
            if (airDensityCurve.keys.Length == 0) airDensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f);
        }

        void FixedUpdate()
        {
            UpdateAtmosphericConditions();
            HandleMovement();
            HandleTurning();
            HandleVerticalMovement();
            if (altitudeHoldEnabled)
                ApplyAltitudeHold();

            ApplyDrag();
            ApplyBuoyancy();
            ApplyStabilization();
            HandleAutoRighting();

            if (enableTurbulence && _isInDangerZone) ApplyAtmosphericTurbulence();
        }

        void ApplyAltitudeHold()
        {
            // PD on altitude
            var y = rb.position.y;
            var vy = rb.linearVelocity.y;

            var error = cruiseAltitudeY - y;
            var accel = holdKp * error - holdKd * vy;

            accel = Mathf.Clamp(accel, -maxHoldAccel, maxHoldAccel);

            // Upward force = mass * accel
            rb.AddForce(Vector3.up * (accel * rb.mass), ForceMode.Force);
        }

        public void EnableAltitudeHold(float targetY)
        {
            cruiseAltitudeY = targetY;
            altitudeHoldEnabled = true;
        }

        public void DisableAltitudeHold()
        {
            altitudeHoldEnabled = false;
        }

        void UpdateAtmosphericConditions()
        {
            var currentHeight = transform.position.y;
            var relativeHeight = currentHeight - _baseHeight;

            // Calculate air density based on height
            if (relativeHeight <= atmosphericFalloffStart)
            {
                _currentAirDensity = 1f; // Full density at low altitudes
            }
            else if (relativeHeight >= maxOperatingHeight)
            {
                _currentAirDensity = minAirDensity; // Minimum density at max height
            }
            else
            {
                // Use curve to interpolate between start and max height
                var normalizedHeight = (relativeHeight - atmosphericFalloffStart) /
                                       (maxOperatingHeight - atmosphericFalloffStart);

                var curveValue = airDensityCurve.Evaluate(normalizedHeight);
                _currentAirDensity = Mathf.Lerp(1f, minAirDensity, 1f - curveValue);
            }

            // Update danger zone status
            _isInDangerZone = relativeHeight > heightWarningThreshold;
        }

        void ApplyAtmosphericTurbulence()
        {
            _turbulenceTimer += Time.fixedDeltaTime;

            // Create turbulence that gets worse with altitude
            var turbulenceFactor = Mathf.InverseLerp(
                heightWarningThreshold, maxOperatingHeight,
                transform.position.y - _baseHeight);

            // Random turbulence forces
            var turbulence = new Vector3(
                Mathf.PerlinNoise(_turbulenceTimer * 2f, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, _turbulenceTimer * 1.5f) - 0.5f,
                Mathf.PerlinNoise(_turbulenceTimer * 1.8f, _turbulenceTimer) - 0.5f
            );

            turbulence *= turbulenceIntensity * turbulenceFactor * (1f - _currentAirDensity);
            rb.AddForce(turbulence, ForceMode.Force);

            // Add rotational instability
            var rotationalTurbulence = new Vector3(
                (Mathf.PerlinNoise(_turbulenceTimer * 3f, 5f) - 0.5f) * 10f,
                0f, // Don't affect Y rotation
                (Mathf.PerlinNoise(5f, _turbulenceTimer * 2.5f) - 0.5f) * 10f
            );

            rotationalTurbulence *= turbulenceFactor * (1f - _currentAirDensity);
            rb.AddTorque(rotationalTurbulence, ForceMode.Force);
        }

        void HandleAutoRighting()
        {
            if (!autoRightAfterCollision) return;

            var currentUp = transform.up;
            var targetUp = Vector3.up;
            var rightingTorque = Vector3.Cross(currentUp, targetUp) * rightingForce;

            // Reduce righting effectiveness in thin air
            rightingTorque *= _currentAirDensity;

            if (rightingTorque.magnitude > 0.01f) rb.AddTorque(rightingTorque, ForceMode.Force);
        }

        void HandleMovement()
        {
            // Calculate base thrust affected by air density
            var targetThrust = thrustValue * (thrustValue >= 0 ? maxForwardThrust : maxBackwardThrust);

            // Air density affects thrust efficiency
            targetThrust *= _currentAirDensity;

            // Engine strain effect - engines work harder but less efficiently in thin air
            if (enableEngineStrain && _currentAirDensity < 0.8f)
            {
                var strainFactor = 1f - _currentAirDensity;
                // Engines consume more "effort" but produce less thrust
                targetThrust *= 1f - strainFactor * 0.5f; // Reduce effectiveness
            }

            currentThrust = Mathf.Lerp(currentThrust, targetThrust, Time.fixedDeltaTime / thrustResponseTime);

            var thrustForce = transform.forward * currentThrust;
            rb.AddForce(thrustForce, ForceMode.Force);

            // Speed-based effectiveness (also affected by air density)
            var speedFactor = Mathf.Clamp01(1f - rb.linearVelocity.magnitude / 30f);
            rb.AddForce(thrustForce * (speedFactor * 0.5f * _currentAirDensity), ForceMode.Force);
        }

        void HandleTurning()
        {
            _currentTurnInput = Mathf.SmoothDamp(_currentTurnInput, turnValue, ref _turnVelocity, turnResponseTime);

            var baseRotationSpeed = _currentTurnInput * 50f;
            var speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / 10f);
            var speedBonus = speedFactor * 15f;
            var totalRotationSpeed = baseRotationSpeed + _currentTurnInput * speedBonus;

            // Air density affects turning responsiveness
            totalRotationSpeed *= _currentAirDensity;

            if (Mathf.Abs(totalRotationSpeed) > 0.01f)
            {
                var deltaRotation = totalRotationSpeed * Time.fixedDeltaTime;
                transform.Rotate(0, deltaRotation, 0, Space.Self);
            }
        }

        void HandleVerticalMovement()
        {
            if (altitudeHoldEnabled)
            {
                // Ignore player vertical input while holding altitude
                _currentVerticalInput = 0f;
                // Keep the tilt logic reacting to forward thrust if you want,
                // or early return here if you prefer zero extra vertical forces.
                return;
            }

            _currentVerticalInput = Mathf.Lerp(
                _currentVerticalInput, changeHeightValue,
                Time.fixedDeltaTime / verticalResponseTime);

            var targetTiltAngle = -thrustValue * maxTiltAngle * 0.5f;
            _currentQuadTiltAngle = Mathf.Lerp(_currentQuadTiltAngle, targetTiltAngle, Time.fixedDeltaTime * 2f);

            // Vertical thrust heavily affected by air density
            var verticalForce = Vector3.up * (_currentVerticalInput * verticalThrustForce * _currentAirDensity);

            var tiltEfficiency = Mathf.Cos(_currentQuadTiltAngle * Mathf.Deg2Rad);
            verticalForce *= tiltEfficiency;

            if (Mathf.Abs(_currentQuadTiltAngle) > 1f)
            {
                var tiltThrust = transform.forward *
                                 (Mathf.Sin(_currentQuadTiltAngle * Mathf.Deg2Rad) * verticalThrustForce * 0.3f *
                                  _currentAirDensity);

                rb.AddForce(tiltThrust, ForceMode.Force);
            }

            rb.AddForce(verticalForce, ForceMode.Force);
        }

        void ApplyDrag()
        {
            // Drag is proportional to air density
            var adjustedDrag = dragCoefficient * _currentAirDensity;
            var dragForce = -rb.linearVelocity * (rb.linearVelocity.magnitude * adjustedDrag * 0.1f);
            rb.AddForce(dragForce, ForceMode.Force);

            // Angular drag also affected by air density
            var adjustedAngularDrag = angularDragCoefficient * _currentAirDensity;
            var angularDragTorque = -rb.angularVelocity * (rb.angularVelocity.magnitude * adjustedAngularDrag * 10f);
            rb.AddTorque(angularDragTorque, ForceMode.Force);
        }

        void ApplyBuoyancy()
        {
            // Buoyancy is affected by air density - less dense air provides less buoyant force
            var adjustedBuoyancy = buoyancyForce * _currentAirDensity;

            var adjustedGravity = Physics.gravity.y * gravityMultiplier;
            var buoyancyVector = Vector3.up * (adjustedBuoyancy + Mathf.Abs(adjustedGravity * rb.mass * 0.8f));
            rb.AddForce(buoyancyVector, ForceMode.Force);
        }

        void ApplyStabilization()
        {
            // Stabilization effectiveness reduced in thin air
            var adjustedStabilization = stabilizationForce * _currentAirDensity;
            var adjustedHoverStabilization = hoverStabilization * _currentAirDensity;

            var lateralVelocity = Vector3.Project(rb.linearVelocity, transform.right);
            var stabilizationForceVector = -lateralVelocity * adjustedStabilization;
            rb.AddForce(stabilizationForceVector, ForceMode.Force);

            var unwantedAngularVel = new Vector3(rb.angularVelocity.x, 0, rb.angularVelocity.z);
            var stabilizationTorque = -unwantedAngularVel * adjustedHoverStabilization;
            rb.AddTorque(stabilizationTorque, ForceMode.Force);
        }

        // Method for future wind implementation
        public void ApplyWind(Vector3 windDirection, float windStrength)
        {
            _windForce = windDirection.normalized * windStrength;
            rb.AddForce(_windForce, ForceMode.Force);
        }

        // Enhanced status method with atmospheric data
        public DirigibleStatus GetStatus()
        {
            return new DirigibleStatus
            {
                velocity = rb.linearVelocity,
                angularVelocity = rb.angularVelocity,
                currentThrust = currentThrust,
                quadTiltAngle = _currentQuadTiltAngle,
                altitude = transform.position.y,
                airDensity = _currentAirDensity,
                isInDangerZone = _isInDangerZone,
                heightAboveBase = transform.position.y - _baseHeight,
                maxSafeHeight = maxOperatingHeight,
                currentVerticalInput = _currentVerticalInput,
                currentTurnInput = _currentTurnInput
            };
        }

        // Public method to check if dirigible can climb safely
        public bool CanClimbSafely()
        {
            var currentRelativeHeight = transform.position.y - _baseHeight;
            return currentRelativeHeight < criticalHeight;
        }

        // Get atmospheric efficiency for UI/audio systems
        public float GetAtmosphericEfficiency()
        {
            return _currentAirDensity;
        }
    }

    // Enhanced data structure for dirigible status
    [Serializable]
    public struct DirigibleStatus
    {
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public float currentThrust;
        public float quadTiltAngle;
        public float altitude;
        public float airDensity;
        public bool isInDangerZone;
        public float heightAboveBase;
        public float maxSafeHeight;
        public float currentVerticalInput;
        public float currentTurnInput;
    }
}
