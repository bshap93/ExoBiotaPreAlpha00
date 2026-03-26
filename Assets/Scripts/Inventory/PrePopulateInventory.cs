using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Inventory
{
    public class PrePopulateInventory : MonoBehaviour
    {
        private MoreMountains.InventoryEngine.Inventory _playerInventory;
        public InventoryItem[] itemsToAdd;


        private void Start()
        {
            _playerInventory = GetComponent<MoreMountains.InventoryEngine.Inventory>();
            // Check if the inventory is not null
            if (_playerInventory == null)
            {
                Debug.LogError("Player's inventory not found!");
                return;
            }

            // Add items to the player's inventory
            foreach (var item in itemsToAdd) _playerInventory.AddItem(item, 1);
        }
    }
}