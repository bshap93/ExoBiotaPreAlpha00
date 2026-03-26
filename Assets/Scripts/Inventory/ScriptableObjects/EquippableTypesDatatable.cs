using System;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Inventory.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EquippableTypesDatatable", menuName = "Inventory/Equippable Types Table")]
    public class EquippableTypesDatatable : ScriptableObject
    {
        [Header("Settings")] [Tooltip("Root folder where InventoryItem assets are stored.")]
        public string itemsRootFolder = "Assets/GameData/Items";

#if ODIN_INSPECTOR
        [TableList]
#endif
        public List<EquippableEntry> entries = new();

        public Dictionary<string, GlobalInventoryManager.EquippableType> ToDictionary()
        {
            var dict = new Dictionary<string, GlobalInventoryManager.EquippableType>();
            foreach (var entry in entries)
                if (!string.IsNullOrEmpty(entry.ItemID) && !dict.ContainsKey(entry.ItemID))
                    dict.Add(entry.ItemID, entry.Type);
            return dict;
        }

        [Serializable]
        public struct EquippableEntry
        {
#if ODIN_INSPECTOR
            [ValueDropdown("GetAllItemIDs")]
#endif
            public string ItemID;

            public GlobalInventoryManager.EquippableType Type;

#if ODIN_INSPECTOR
            /// <summary>
            ///     Scans only the folder specified in the parent ScriptableObject.
            /// </summary>
            private IEnumerable<string> GetAllItemIDs()
            {
#if UNITY_EDITOR
                // Access the parent ScriptableObject
                var parent = Selection.activeObject as EquippableTypesDatatable;
                var searchPath = parent != null ? parent.itemsRootFolder : "Assets";

                var guids = AssetDatabase.FindAssets("t:InventoryItem", new[] { searchPath });
                var ids = new List<string>();

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var item = AssetDatabase.LoadAssetAtPath<InventoryItem>(path);
                    if (item != null && !string.IsNullOrEmpty(item.ItemID))
                        ids.Add(item.ItemID);
                }

                ids.Sort();
                return ids;
#else
                return new List<string>();
#endif
            }
#endif
        }
    }
}