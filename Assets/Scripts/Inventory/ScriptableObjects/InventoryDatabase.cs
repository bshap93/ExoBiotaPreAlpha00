using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Inventory.ScriptableObjects
{
    [CreateAssetMenu(fileName = "InventoryDatabase", menuName = "Scriptable Objects/Inventory/Inventory Database")]
    public class InventoryDatabase : SerializedScriptableObject
    {
        Dictionary<string, InventoryItem> _lookup;

        [LabelText("All Items (Auto-Populated from Resources/Items)")] [ReadOnly]
        // so you don't edit directly — list is managed automatically
        public List<InventoryItem> items = new();

        void OnEnable()
        {
            AutoPopulateFromResources();
            BuildLookup();
        }

        [Button("Auto-Populate From Resources", ButtonSizes.Large)]
        void AutoPopulateFromResources()
        {
            // Load all InventoryItem assets in Resources/Items (including subfolders)
            items = Resources.LoadAll<InventoryItem>("Items")
                .Where(i => i != null)
                .OrderBy(i => i.ItemID)
                .ToList();
        }

        [Button("Rebuild Lookup", ButtonSizes.Small)]
        void BuildLookup()
        {
            _lookup = new Dictionary<string, InventoryItem>();
            foreach (var item in items)
            {
                if (_lookup.ContainsKey(item.ItemID)) continue;

                _lookup[item.ItemID] = item;
            }
        }

        /// <summary>
        ///     Returns a COPY of the item with the given ID and quantity, or null if not found.
        /// </summary>
        public InventoryItem CreateItem(string itemId, int quantity = 1)
        {
            if (_lookup == null || _lookup.Count != items.Count)
                BuildLookup();

            if (!_lookup.TryGetValue(itemId, out var original))
            {
                Debug.LogError($"[InventoryDatabase] No item found with ID '{itemId}'.");
                return null;
            }

            var copy = original.Copy();
            copy.Quantity = Mathf.Max(1, quantity);
            return copy;
        }

        /// <summary>
        ///     Returns the original asset (don't modify this at runtime).
        /// </summary>
        public MyBaseItem GetItemAsset(string itemId)
        {
            if (_lookup == null || _lookup.Count != items.Count)
                BuildLookup();

            _lookup.TryGetValue(itemId, out var original);
            return original as MyBaseItem;
        }
    }
}
