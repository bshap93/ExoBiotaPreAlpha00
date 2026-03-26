using System;
using CompassNavigatorPro;
using UnityEngine;

namespace Scanning
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CompassProPOI))]
    public class MineSectionPOI : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Tooltip("Unique ID for this mine section.")]
        public string sectionId;

#if UNITY_EDITOR
        // Helper to avoid empty IDs in editor
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(sectionId))
                sectionId = gameObject.scene.name + "_" + gameObject.name;
        }
#endif
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (Application.isEditor && string.IsNullOrEmpty(sectionId))
                sectionId = Guid.NewGuid().ToString();
// #if UNITY_EDITOR
//             EditorUtility.SetDirty(this);
// #endif
        }
    }
}