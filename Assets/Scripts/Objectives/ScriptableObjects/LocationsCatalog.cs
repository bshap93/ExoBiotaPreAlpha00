using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities.Editor;
#endif

namespace Objectives.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "LocationsCatalog",
        menuName = "Scriptable Objects/Objectives/LocationsCatalog")]
    public class LocationsCatalog : ScriptableObject
    {
        [ListDrawerSettings(
            HideAddButton = true,
            DraggableItems = false, OnTitleBarGUI = "DrawCreateBtn")]
        [InlineEditor(InlineEditorModes.FullEditor)]
        public List<SubjectLocationObject> locations = new();
        public string catalogName = "Default";

#if UNITY_EDITOR
        void DrawCreateBtn()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
            {
                var obj = CreateInstance<SubjectLocationObject>();
                obj.name = $"{catalogName}_{locations.Count}";
                obj.locationName = obj.name;
                obj.associatedPOIUniqueId = Guid.NewGuid().ToString();

                AssetDatabase.AddObjectToAsset(obj, this);
                AssetDatabase.SaveAssets();
                locations.Add(obj);
            }
        }
#endif
    }
}
