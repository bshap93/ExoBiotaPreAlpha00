using FirstPersonPlayer.Tools.ItemObjectTypes;
using Inventory;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Static;

namespace SharedUI.Hotbar
{
    [DisallowMultipleComponent]
    public class HotbarUISlot : MonoBehaviour
    {
        public enum HotbarSlotType
        {
            Tool,
            Hands,
            EmptySlot,
            Consumable
        }

        [SerializeField] Sprite handsIcon;
        [SerializeField] Image slotBG;
        [SerializeField] Image secondaryBG;
        [SerializeField] Image itemIcon;
        [SerializeField] bool quantityEnabled;
        [ShowIf("quantityEnabled")] [SerializeField]
        TMP_Text quantityText;
        [ShowIf("quantityEnabled")] [SerializeField]
        GameObject quantityBadge;
        [ShowIf("quantityEnabled")] [SerializeField]
        Color moreThanZeroColor;
        [ShowIf("quantityEnabled")] [SerializeField]
        Color zeroColor;

        [Header("Visual Feedback")] [SerializeField]
        Color selectedColor = Color.yellow;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] float selectedScale = 1.1f;
        string _currentItemID;

        HotbarSlotType _currentType;
        bool _isSelected;

        void Start()
        {
            // Initialize as empty
            ClearSlot();
        }

        public void UpdateSlot(string itemID, int quantity, Sprite icon, HotbarSlotType slotType)
        {
            _currentItemID = itemID;
            _currentType = slotType;

            if (string.IsNullOrEmpty(itemID))
            {
                ClearSlot();
                return;
            }

            // Set icon
            if (itemIcon != null && icon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.enabled = true;
                itemIcon.color = Color.white;
            }

            // Set quantity if enabled
            if (quantityEnabled && quantityText != null && quantityBadge != null)
            {
                if (slotType == HotbarSlotType.Consumable && quantity > 0)
                {
                    quantityText.text = quantity.ToString();
                    quantityBadge.SetActive(true);

                    // Update color based on quantity
                    quantityText.color = quantity > 0 ? moreThanZeroColor : zeroColor;
                }
                else
                {
                    quantityBadge.SetActive(false);
                }
            }
        }

        public void UpdateSlotFromInventory(string itemID, HotbarSlotType slotType)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                ClearSlot();
                return;
            }

            var inventoryManager = GlobalInventoryManager.Instance;
            if (inventoryManager == null || inventoryManager.playerInventory == null)
            {
                Debug.LogWarning("[HotbarUISlot] GlobalInventoryManager or playerInventory is null.");
                ClearSlot();
                return;
            }

            // Find the item in inventory
            MyBaseItem item = null;
            var totalQuantity = 0;

            foreach (var invItem in inventoryManager.playerInventory.Content)
                if (invItem != null && invItem.ItemID == itemID)
                {
                    if (item == null) item = invItem as MyBaseItem;
                    totalQuantity += invItem.Quantity;
                }

            if (item != null)
                UpdateSlot(itemID, totalQuantity, item.GetDisplayIcon(), slotType);
            else
                ClearSlot();
        }

        public void ClearSlot()
        {
            _currentItemID = string.Empty;
            _currentType = HotbarSlotType.EmptySlot;

            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (quantityEnabled && quantityBadge != null) quantityBadge.SetActive(false);

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (slotBG != null) slotBG.color = selected ? selectedColor : normalColor;

            if (secondaryBG != null) secondaryBG.color = selected ? selectedColor : normalColor;

            // Optional: Scale effect
            if (transform is RectTransform rectTransform)
                rectTransform.localScale = selected ? Vector3.one * selectedScale : Vector3.one;
        }

        public void SetEmptyHandSlot()
        {
            _currentType = HotbarSlotType.Hands;
            _currentItemID = string.Empty;

            if (itemIcon != null)
                // You can set a specific "hands" icon here if you have one
                itemIcon.sprite = handsIcon;

            // itemIcon.enabled = false;
            if (quantityEnabled && quantityBadge != null) quantityBadge.SetActive(false);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(_currentItemID) && _currentType != HotbarSlotType.Hands;
        }

        public string GetCurrentItemID()
        {
            return _currentItemID;
        }

        public HotbarSlotType GetSlotType()
        {
            return _currentType;
        }
    }
}
