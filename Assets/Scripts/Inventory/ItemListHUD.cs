using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.Interface;
using UnityEngine;

namespace Inventory
{
    public class ItemListHUD : MonoBehaviour, MMEventListener<MMInventoryEvent>, IItemList
    {
        public GameObject list;
        public GameObject itemElementPrefab;
        public List<GameObject> itemElements = new();
        public MoreMountains.InventoryEngine.Inventory mainInventory;

        void Start()
        {
            RefreshItemList();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void RefreshItemList()
        {
            // 1. clear the previous rows
            foreach (var go in itemElements) Destroy(go);
            itemElements.Clear();

            if (mainInventory == null || mainInventory.Content == null) return;
            // Clear existing item elements
            foreach (var element in itemElements) Destroy(element);
            itemElements.Clear();

            // Get grouped items directly from the inventory
            var inventoryItems = mainInventory.Content;

            // 2. rebuild the list
            foreach (var slot in mainInventory.Content)
            {
                if (InventoryItem.IsNull(slot) || slot.Quantity <= 0) continue;

                var row = Instantiate(itemElementPrefab, list.transform);
                itemElements.Add(row);

                // Fill UI
                if (row.TryGetComponent(out ItemElement ui))
                {
                    ui.ItemImage.sprite = slot.Icon; // or whatever field holds the icon
                    ui.ItemQuantity.text = slot.Quantity.ToString();
                }
            }
        }

        public void OnMMEvent(MMInventoryEvent e)
        {
            // Ignore events that are not for this inventory / player
            if (e.TargetInventoryName != mainInventory?.name || e.PlayerID != mainInventory?.PlayerID)
                return;

            if (e.InventoryEventType == MMInventoryEventType.ContentChanged)
                RefreshItemList();
        }
    }

// Helper component to store reference data on UI elements
    public class ItemElementData : MonoBehaviour
    {
        public string ItemID;
        public List<string> UniqueIDs;
    }
}
