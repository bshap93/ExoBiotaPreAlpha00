using System.Linq;
using UnityEditor;
using UnityEngine;
using Utilities.Interface;
using Object = UnityEngine.Object;

namespace Utilities.Static
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class PrefabAddedDetector
    {
#if UNITY_EDITOR
        static PrefabAddedDetector()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
#endif

        static void OnHierarchyChanged()
        {
            // var itemPickers = Object.FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);
            // foreach (var itemPicker in itemPickers)
            //     if (itemPicker.uniqueID.IsEmpty())
            //         itemPicker.SetUniqueID();

            // Single call to find all objects that need unique IDs
            var objectsNeedingIDs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IRequiresUniqueID>()
                .Where(obj => obj.IsUniqueIDEmpty());

            foreach (var obj in objectsNeedingIDs)
                obj.SetUniqueID();
        }
    }
}
