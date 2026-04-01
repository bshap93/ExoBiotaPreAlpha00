using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objectives.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(
        fileName = "SubjectLocationObject", menuName = "Scriptable Objects/Objectives/SubjectLocationObject",
        order = 3)]
    public class SubjectLocationObject : ScriptableObject
    {
        [FormerlySerializedAs("LocationName")] public string locationName;
        public string associatedPOIUniqueId;


#if UNITY_EDITOR
        string _lastKnownName;

        void OnValidate()
        {
            // Only assign once, when it's empty
            if (string.IsNullOrEmpty(associatedPOIUniqueId))
            {
                associatedPOIUniqueId = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }

            // Detect if the asset was renamed externally (header bar, AssetDatabase, etc.)
            if (_lastKnownName != name)
            {
                locationName = name; // sync ID to match external rename
                _lastKnownName = name;
                EditorUtility.SetDirty(this);
            }
        }

        void OnObjectiveIdChanged()
        {
            if (string.IsNullOrWhiteSpace(locationName)) return;

            // Sync asset name to ID
            name = locationName;
            _lastKnownName = name;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [Button("Rename Asset ↔ ID", ButtonSizes.Small)]
        void RenameAssetToId()
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                Debug.LogWarning("[ObjectiveObject] objectiveId is empty; rename cancelled.");
                return;
            }

            name = locationName; // ensures sub-asset name matches the field
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
