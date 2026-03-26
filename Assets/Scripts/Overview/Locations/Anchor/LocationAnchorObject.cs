using System;
using OWPData.ScriptableObjects;
using UnityEngine;

namespace Overview.Locations.Anchor
{
    [Serializable]
    public class LocationAnchorObject : MonoBehaviour
    {
        // public DockOvLocationDefinition dockOvLocationDefinition;
        public Transform locationCameraAnchor;

        // [SerializeField] bool overrideAnchorLocation = true;
        [SerializeField] Vector3 anchorLocationOverride;
        [SerializeField] Vector3 anchorRotationOverride;

        // public string GetLocationId()
        // {
        //     return dockOvLocationDefinition != null ? dockOvLocationDefinition.locationId : string.Empty;
        // }
        //
        // public string GetDockId()
        // {
        //     return dockOvLocationDefinition != null ? dockOvLocationDefinition.dockId : string.Empty;
        // }
    }
}
