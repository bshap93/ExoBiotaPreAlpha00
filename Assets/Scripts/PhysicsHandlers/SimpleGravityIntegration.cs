using UnityEngine;

namespace PhysicsHandlers
{
    public static class SimpleGravityIntegration
    {
        /// <summary>
        ///     Call this after digging operations
        /// </summary>
        public static void OnDigPerformed(Vector3 digPosition, float radius = 5f)
        {
            SimpleGravityManager.Instance?.CheckNearbyObjects(digPosition, radius);
        }
    }
}