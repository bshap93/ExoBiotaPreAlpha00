using System;
using Objectives.ScriptableObjects;
using UnityEngine;

namespace Helpers.Wrappers
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    namespace Objectives.ScriptableObjects
    {
        public static class SubjectLocationFactory
        {
#if UNITY_EDITOR
            public static SubjectLocationObject CreateFromPOI(GamePOIWrapper wrapper, string folderPath)
            {
                if (wrapper == null) return null;

                var asset = ScriptableObject.CreateInstance<SubjectLocationObject>();

                // Sensible name: use the GameObject or POI name
                asset.name = wrapper.name + "_Location";
                asset.locationName = wrapper.name;

                // UniqueID: generate new if empty
                asset.associatedPOIUniqueId =
                    string.IsNullOrEmpty(wrapper.UniqueID)
                        ? Guid.NewGuid().ToString()
                        : wrapper.UniqueID;

                // Save into Assets folder
                var path = $"{folderPath}/{asset.name}.asset";
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Hook the wrapper to this new SO
                wrapper.locationObject = asset;

                return asset;
            }
#endif
        }
    }
}
