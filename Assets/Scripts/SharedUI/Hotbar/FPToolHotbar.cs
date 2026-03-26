using Helpers.Events.UI;
using Manager.UI;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Hotbar
{
    [DisallowMultipleComponent]
    public class FPToolHotbar : MonoBehaviour, MMEventListener<HotbarEvent>
    {
        // 6 total: slot 0 = empty hand, slots 1-5 = tools
        [SerializeField] int hotbarSize = 6;
        [SerializeField] HotbarUISlot emptySlot; // Slot 0 - empty hands
        [SerializeField] HotbarUISlot[] toolSlots; // Slots 1-5 - tools

        int _currentSelectedIndex; // Start with empty hands

        void Start()
        {
            ValidateSlots();
            ValidateSetup();

            // Set up empty hand slot
            if (emptySlot != null)
            {
                emptySlot.SetEmptyHandSlot();
                emptySlot.SetSelected(true); // Start with empty hands selected
            }

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
                case HotbarEvent.HotbarEventType.ToolHotbarChanged:
                    UpdateSlot(eventType.SlotIndex, eventType.ItemID);
                    break;

                case HotbarEvent.HotbarEventType.SelectToolSlot:
                    SelectSlot(eventType.SlotIndex);
                    break;
            }
        }

        void ValidateSetup()
        {
            // Debug.Log("=== FPToolHotbar Setup Validation ===");
            // Debug.Log($"hotbarSize: {hotbarSize}");
            // Debug.Log($"emptySlot assigned: {emptySlot != null}");
            // Debug.Log($"toolSlots.Length: {toolSlots?.Length ?? 0}");

            // if (toolSlots != null)
            //     for (var i = 0; i < toolSlots.Length; i++)
            //         if (toolSlots[i] != null)
            //             Debug.Log($"toolSlots[{i}] = {toolSlots[i].gameObject.name} (Key {i + 4})");
            //         else
            //             Debug.LogError($"toolSlots[{i}] is NULL! Assign it in inspector!");

            // Check if emptySlot is accidentally in toolSlots
            if (emptySlot != null && toolSlots != null)
                for (var i = 0; i < toolSlots.Length; i++)
                    if (toolSlots[i] == emptySlot)
                        Debug.LogError(
                            $"ERROR: emptySlot is assigned to toolSlots[{i}]! " +
                            "The emptySlot should be separate from toolSlots array. " +
                            "Remove it from toolSlots!");

            // Debug.Log("=== End Validation ===");
        }

        void ValidateSlots()
        {
            if (emptySlot == null) Debug.LogError("[FPToolHotbar] emptySlot is not assigned!");

            if (toolSlots == null || toolSlots.Length != hotbarSize - 1) // -1 because emptySlot is separate
                Debug.LogError(
                    $"[FPToolHotbar] toolSlots array size ({toolSlots?.Length ?? 0}) should be {hotbarSize - 1}!");
        }

        void UpdateSlot(int dataIndex, string itemID)
        {
            // dataIndex is the index in HotbarManager's _fpToolHotbarItems array
            // dataIndex 0 = empty hands (key 3)
            // dataIndex 1 = first tool (key 4) -> should update toolSlots[0]
            // dataIndex 2 = second tool (key 5) -> should update toolSlots[1]
            // dataIndex 3 = third tool (key 6) -> should update toolSlots[2]

            if (dataIndex < 0 || dataIndex >= hotbarSize)
            {
                Debug.LogWarning($"[FPToolHotbar] Invalid data index: {dataIndex}");
                return;
            }

            HotbarUISlot slot = null;

            // Data index 0 is for empty hands (key 3)
            if (dataIndex == 0)
            {
                slot = emptySlot;
                if (slot != null) slot.SetEmptyHandSlot();
                return;
            }

            // Data indices 1-5 are for tools (keys 4-6)
            // Map to toolSlots array: dataIndex 1 -> toolSlots[0], etc.
            var toolArrayIndex = dataIndex - 1;
            if (toolArrayIndex >= 0 && toolArrayIndex < toolSlots.Length)
            {
                slot = toolSlots[toolArrayIndex];
            }
            else
            {
                Debug.LogWarning(
                    $"[FPToolHotbar] toolArrayIndex {toolArrayIndex} out of range for toolSlots.Length {toolSlots.Length}");

                return;
            }

            if (slot == null)
            {
                Debug.LogWarning(
                    $"[FPToolHotbar] Slot at toolSlots[{toolArrayIndex}] is null! Check your inspector assignments.");

                return;
            }

            if (string.IsNullOrEmpty(itemID))
                slot.ClearSlot();
            else
                slot.UpdateSlotFromInventory(itemID, HotbarUISlot.HotbarSlotType.Tool);
        }

        void SelectSlot(int slotIndex)
        {
            // Deselect previous
            if (_currentSelectedIndex == 0 && emptySlot != null)
            {
                emptySlot.SetSelected(false);
            }
            else if (_currentSelectedIndex > 0)
            {
                var toolArrayIndex = _currentSelectedIndex - 1;
                if (toolArrayIndex >= 0 && toolArrayIndex < toolSlots.Length && toolSlots[toolArrayIndex] != null)
                    toolSlots[toolArrayIndex].SetSelected(false);
            }

            // Select new
            _currentSelectedIndex = slotIndex;

            if (_currentSelectedIndex == 0 && emptySlot != null)
            {
                emptySlot.SetSelected(true);
            }
            else if (_currentSelectedIndex > 0)
            {
                var toolArrayIndex = _currentSelectedIndex - 1;
                if (toolArrayIndex >= 0 && toolArrayIndex < toolSlots.Length && toolSlots[toolArrayIndex] != null)
                    toolSlots[toolArrayIndex].SetSelected(true);
            }
        }

        public void RefreshAllSlots()
        {
            var hotbarManager = HotbarManager.Instance;
            if (hotbarManager == null)
            {
                Debug.LogWarning("[FPToolHotbar] HotbarManager.Instance is null!");
                return;
            }

            // Refresh empty hand slot (data index 0, key 3)
            if (emptySlot != null)
                emptySlot.SetEmptyHandSlot();
            else
                Debug.LogError("[FPToolHotbar] emptySlot is null! Assign it in the inspector.");

            // Refresh tool slots
            // toolSlots[i] corresponds to data index (i+1) and key (i+4)
            for (var i = 0; i < toolSlots.Length; i++)
            {
                var dataIndex = i + 1; // +1 because data index 0 is empty hand
                var keyNumber = i + 4; // +4 because keys start at 4 for tools

                var hotbarData = hotbarManager.GetToolAtSlot(dataIndex);
                var itemID = hotbarData?.itemID;


                UpdateSlot(dataIndex, itemID);
            }
        }

        public void HandleToolKeyPress(int keyIndex)
        {
            // keyIndex: 0 = empty hand (key 3), 1-3 = tools (keys 4-6)
            if (keyIndex < 0 || keyIndex >= hotbarSize) return;

            var hotbarManager = HotbarManager.Instance;
            if (hotbarManager == null)
            {
                Debug.LogWarning("[FPToolHotbar] HotbarManager.Instance is null!");
                return;
            }

            // Equip the tool (or unequip if keyIndex is 0)
            hotbarManager.EquipToolAtSlot(keyIndex);

            // Visual feedback
            SelectSlot(keyIndex);
        }

        public int GetCurrentSelectedIndex()
        {
            return _currentSelectedIndex;
        }

        public void CycleTools(int direction)
        {
            // direction: 1 = forward (scroll up), -1 = backward (scroll down)

            // Calculate new index with wrapping
            var newIndex = _currentSelectedIndex + direction;

            // Wrap around
            if (newIndex < 0)
                newIndex = hotbarSize - 1; // Wrap to last slot
            else if (newIndex >= hotbarSize) newIndex = 0; // Wrap to first slot (empty hands)

            Debug.Log($"[FPToolHotbar] Cycling from slot {_currentSelectedIndex} to slot {newIndex}");

            // Trigger equip for the new slot
            HandleToolKeyPress(newIndex);
        }
    }
}
