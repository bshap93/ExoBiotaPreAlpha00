using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Inventory.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DefaultInventoryDefinition",
        menuName = "Scriptable Objects/Inventory/Default Inventory Definition")]
    public class DefaultInventoryDefinition : ScriptableObject
    {
        public int inventorySize;
        public InventoryItem[] defaultItems;
    }
}