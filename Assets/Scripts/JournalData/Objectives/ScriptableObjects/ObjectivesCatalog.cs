// Editor-only assemblies:

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace Objectives.ScriptableObjects
{
    public enum ObjectiveCategoryType
    {
        Tutorial,
        MainStory,
        Peripheral
    }

    public enum ObjectiveType
    {
        Collect,
        TalkTo,
        Visit,
        Interact,
        ExploreArea,
        Equipment
    }

    [CreateAssetMenu(
        fileName = "ObjectivesCatalogue",
        menuName = "Scriptable Objects/Objectives/Catalog")]
    public class ObjectivesCatalog : ScriptableObject
    {
        [Header("Catalog Settings")] public ObjectiveCategoryType catalogCategory;


        [ListDrawerSettings(
            HideAddButton = true,
            DraggableItems = false, OnTitleBarGUI = "DrawCreateBtn")]
        [InlineEditor(InlineEditorModes.FullEditor)]
        public List<ObjectiveObject> objectives = new();


#if UNITY_EDITOR
        void DrawCreateBtn()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
            {
                var obj = CreateInstance<ObjectiveObject>();
                obj.name = $"Objective_{objectives.Count}";
                obj.catalogContext = catalogCategory;

                AssetDatabase.AddObjectToAsset(obj, this);
                objectives.Add(obj);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(5);

            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Minus))
                if (objectives.Count > 0)
                {
                    var last = objectives[^1];
                    objectives.RemoveAt(objectives.Count - 1);
                    DestroyImmediate(last, true);
                    AssetDatabase.SaveAssets();
                }
        }
#endif
// #if UNITY_EDITOR
//         void DrawCreateBtn()
//         {
//             if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
//             {
//                 var obj = CreateInstance<ObjectiveObject>();
//                 obj.name = $"Objective_{objectives.Count}";
//                 obj.catalogContext = catalogCategory; // optional tag
// // objectiveId now auto-syncs inside OnValidate()
//
//                 AssetDatabase.AddObjectToAsset(obj, this);
//                 AssetDatabase.SaveAssets();
//                 objectives.Add(obj);
//             }
//         }
// #endif
    }
}
