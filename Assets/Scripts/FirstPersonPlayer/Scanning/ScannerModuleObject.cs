using UnityEngine;

namespace Inventory.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ScannerModule", menuName = "Inventory/Scanner Module")]
    public class ScannerModuleObject : ScriptableObject
    {
        public string moduleId; // e.g., "metal", "energy", "bio"
        public LayerMask detectableLayers; // what OverlapSphere will look for
        public float pingRadius = 12f; // radius for detection ping
        public float maxMarkers = 10; // cap the # of highlighted results
        public Color uiTint = Color.cyan; // optional tint for UI feedbacks
        public bool requireLineOfSight = true; // do a second ray to confirm LoS
    }
}