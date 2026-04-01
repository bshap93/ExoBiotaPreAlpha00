using System;
using System.Collections.Generic;
using Objectives.ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace JournalData.Objectives.ScriptableObjects
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
