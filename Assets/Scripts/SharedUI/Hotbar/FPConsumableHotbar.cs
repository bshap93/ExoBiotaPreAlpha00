using Helpers.Events.UI;
using Manager.UI;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Hotbar
{
    [DisallowMultipleComponent]
    public class FPConsumableHotbar : MonoBehaviour, MMEventListener<HotbarEvent>
    {
        [SerializeField] int hotbarSize = 2;
        [SerializeField] HotbarUISlot[] consumableSlots;

        int _currentSelectedIndex = -1;

        void Start()
        {
            ValidateSlots();
            RefreshAllSlots();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(HotbarEvent eventType)
        {
            switch (eventType.EventType)
            {
                case HotbarEvent.HotbarEventType.ConsumableHotbarChanged:
                    UpdateSlot(eventType.SlotIndex, eventType.ItemID);
                    break;

                case HotbarEvent.HotbarEventType.SelectConsumableSlot:
                    SelectSlot(eventType.SlotIndex);
                    break;
            }
        }

        void ValidateSlots()
        {
            if (consumableSlots == null || consumableSlots.Length != hotbarSize)
                Debug.LogError(
                    $"[FPConsumableHotbar] consumableSlots array size ({consumableSlots?.Length ?? 0}) doesn't match hotbarSize ({hotbarSize})!");
        }

        void UpdateSlot(int slotIndex, string itemID)
        {
            if (slotIndex < 0 || slotIndex >= consumableSlots.Length)
            {
                Debug.LogWarning($"[FPConsumableHotbar] Invalid slot index: {slotIndex}");
                return;
            }

            var slot = consumableSlots[slotIndex];
            if (slot == null)
            {
                Debug.LogWarning($"[FPConsumableHotbar] Slot at index {slotIndex} is null!");
                return;
            }

            if (string.IsNullOrEmpty(itemID))
                slot.ClearSlot();
            else
                slot.UpdateSlotFromInventory(itemID, HotbarUISlot.HotbarSlotType.Consumable);
        }

        void SelectSlot(int slotIndex)
        {
            // Deselect previous
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < consumableSlots.Length)
                consumableSlots[_currentSelectedIndex]?.SetSelected(false);

            // Select new
            _currentSelectedIndex = slotIndex;
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < consumableSlots.Length)
                consumableSlots[_currentSelectedIndex]?.SetSelected(true);
        }

        public void RefreshAllSlots()
        {
            var hotbarManager = HotbarManager.Instance;
            if (hotbarManager == null)
            {
                Debug.LogWarning("[FPConsumableHotbar] HotbarManager.Instance is null!");
                return;
            }

            for (var i = 0; i < consumableSlots.Length; i++)
            {
                var hotbarData = hotbarManager.GetConsumableAtSlot(i);
                var itemID = hotbarData?.itemID;
                UpdateSlot(i, itemID);
            }
        }

        public void HandleConsumableKeyPress(int keyIndex)
        {
            // keyIndex is 0-based (0 for key 1, 1 for key 2)
            if (keyIndex < 0 || keyIndex >= consumableSlots.Length) return;

            var hotbarManager = HotbarManager.Instance;
            if (hotbarManager == null)
            {
                Debug.LogWarning("[FPConsumableHotbar] HotbarManager.Instance is null!");
                return;
            }

            // Use the consumable
            hotbarManager.UseConsumableAtSlot(keyIndex);

            // Visual feedback - briefly select the slot
            SelectSlot(keyIndex);
            Invoke(nameof(DeselectAll), 0.2f);
        }

        void DeselectAll()
        {
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < consumableSlots.Length)
                consumableSlots[_currentSelectedIndex]?.SetSelected(false);

            _currentSelectedIndex = -1;
        }
    }
}
