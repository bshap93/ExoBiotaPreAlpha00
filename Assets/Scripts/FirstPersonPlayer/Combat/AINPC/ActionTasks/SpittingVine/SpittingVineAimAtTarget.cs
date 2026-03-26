using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.SpittingVine
{
    [Category("AttackMoves")]
    [Description("Procedurally aims a spitting plant by bending its trunk segments toward the target")]
    public class SpittingVineAimAtTarget : ActionTask
    {
        [RequiredField]
        public BBParameter<GameObject> AimTarget = null;
        
        [RequiredField]
        public BBParameter<float> AimDuration = 1f;
        
        [Tooltip("Name/prefix of the trunk bone transforms (e.g., 'Trunk_' will find Trunk_01, Trunk_02, etc.)")]
        public BBParameter<string> TrunkBonePrefix = "Trunk_";
        
        [Tooltip("Number of trunk segments (excluding the flower)")]
        public BBParameter<int> TrunkSegmentCount = 8;
        
        [Tooltip("Controls bend distribution curve: 0=linear (stiff), 1=steep curve (natural plant bend)")]
        [SliderField(0f, 1f)]
        public BBParameter<float> BendIntensity = 0.8f;

        private Transform[] _trunkSegments;
        private Quaternion[] _initialRotations;
        private Quaternion[] _targetRotations;
        private float _elapsedTime;
        private Transform _targetTransform;
        private bool _isInitialized;

        protected override string info
        {
            get { return $"Aim at {AimTarget} over {AimDuration}s"; }
        }

        protected override void OnExecute()
        {
            _elapsedTime = 0f;
            _isInitialized = false;

            // Validate target
            if (AimTarget.value == null)
            {
                Debug.LogWarning("SpittingVineAimAtTarget: AimTarget is null!");
                EndAction(false);
                return;
            }

            _targetTransform = AimTarget.value.transform;

            // Find trunk segments
            if (!InitializeTrunkSegments())
            {
                Debug.LogWarning("SpittingVineAimAtTarget: Failed to find trunk segments!");
                EndAction(false);
                return;
            }

            // Store initial rotations
            _initialRotations = new Quaternion[_trunkSegments.Length];
            _targetRotations = new Quaternion[_trunkSegments.Length];
            
            for (int i = 0; i < _trunkSegments.Length; i++)
            {
                _initialRotations[i] = _trunkSegments[i].localRotation;
            }

            _isInitialized = true;
        }

        protected override void OnUpdate()
        {
            if (!_isInitialized)
            {
                EndAction(false);
                return;
            }

            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / AimDuration.value);
            
            // Use smooth step for more natural motion
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Calculate target rotations for each segment
            CalculateTargetRotations();

            // Apply interpolated rotations
            for (int i = 0; i < _trunkSegments.Length; i++)
            {
                Quaternion currentRot = Quaternion.Slerp(_initialRotations[i], _targetRotations[i], smoothT);
                _trunkSegments[i].localRotation = currentRot;
            }

            // Check if aiming is complete
            if (t >= 1f)
            {
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            // Clean up
            _trunkSegments = null;
            _initialRotations = null;
            _targetRotations = null;
            _isInitialized = false;
        }

        private bool InitializeTrunkSegments()
        {
            int segmentCount = TrunkSegmentCount.value;
            _trunkSegments = new Transform[segmentCount];

            // Try to find trunk segments by name
            for (int i = 0; i < segmentCount; i++)
            {
                // Try common naming conventions
                string[] possibleNames = new string[]
                {
                    $"{TrunkBonePrefix.value}{(i + 1):D2}",  // Trunk_01, Trunk_02, etc.
                    $"{TrunkBonePrefix.value}{i + 1}",        // Trunk_1, Trunk_2, etc.
                    $"{TrunkBonePrefix.value}{i:D2}",         // Trunk_00, Trunk_01, etc.
                    $"{TrunkBonePrefix.value}{i}"             // Trunk_0, Trunk_1, etc.
                };

                Transform foundSegment = null;
                foreach (string name in possibleNames)
                {
                    foundSegment = FindTransformRecursive(agent.transform, name);
                    if (foundSegment != null)
                        break;
                }

                if (foundSegment == null)
                {
                    Debug.LogWarning($"SpittingVineAimAtTarget: Could not find trunk segment '{possibleNames[0]}'");
                    return false;
                }

                _trunkSegments[i] = foundSegment;
            }

            return true;
        }

        private Transform FindTransformRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            foreach (Transform child in parent)
            {
                Transform result = FindTransformRecursive(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void CalculateTargetRotations()
        {
            if (_targetTransform == null || _trunkSegments == null)
                return;

            // Calculate bend for each segment
            // Each segment bends toward the target, with increasing influence up the chain
            for (int i = 0; i < _trunkSegments.Length; i++)
            {
                Transform segment = _trunkSegments[i];
                
                // Get direction to target from this segment's position
                Vector3 directionToTarget = (_targetTransform.position - segment.position).normalized;
                
                // Calculate the base forward direction (local -X in local space = global +Y)
                // Since segments extend in local -X, we want to rotate around to aim at target
                Vector3 currentForward = segment.TransformDirection(Vector3.left); // Local -X
                
                // Calculate rotation needed to point toward target
                Quaternion lookRotation = Quaternion.FromToRotation(currentForward, directionToTarget);
                
                // IMPROVED: Better segment influence curve
                // Lower segments bend less, upper segments bend more
                float normalizedPosition = (float)(i + 1) / _trunkSegments.Length; // 0.125 to 1.0 for 8 segments
                
                // Use BendIntensity to control the curve steepness
                // Higher BendIntensity = steeper curve = more natural plant bend
                float curveExponent = 1.0f + BendIntensity.value * 3.0f; // Range: 1.0 to 4.0
                float segmentInfluence = Mathf.Pow(normalizedPosition, curveExponent);
                
                // Blend between current rotation and look rotation based on influence
                Quaternion targetWorldRotation = Quaternion.Slerp(segment.rotation, lookRotation * segment.rotation, segmentInfluence);
                
                // Convert back to local space
                if (segment.parent != null)
                {
                    _targetRotations[i] = Quaternion.Inverse(segment.parent.rotation) * targetWorldRotation;
                }
                else
                {
                    _targetRotations[i] = targetWorldRotation;
                }
            }
        }

        // Optional: You can access the current aiming direction for projectile firing
        public Vector3 GetAimDirection()
        {
            if (_trunkSegments != null && _trunkSegments.Length > 0)
            {
                // Return the forward direction of the topmost segment (closest to flower)
                Transform topSegment = _trunkSegments[_trunkSegments.Length - 1];
                return topSegment.TransformDirection(Vector3.left); // Local -X direction
            }
            return Vector3.forward;
        }

        // Optional: Get the position where projectile should spawn (flower position)
        public Vector3 GetProjectileSpawnPoint()
        {
            if (_trunkSegments != null && _trunkSegments.Length > 0)
            {
                // The flower is above the last trunk segment
                Transform topSegment = _trunkSegments[_trunkSegments.Length - 1];
                // Approximate flower position (0.2 units in local -X from last segment)
                return topSegment.position + topSegment.TransformDirection(Vector3.left * 0.2f);
            }
            return agent.transform.position;
        }
    }
}