using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Inventory.UI
{
    public abstract class ItemElementBase : MonoBehaviour
    {
        public abstract void Bind(InventoryItem item);
    }
}