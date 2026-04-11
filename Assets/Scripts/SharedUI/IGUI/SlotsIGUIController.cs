using Helpers.Events;
using Helpers.Events.UI;
using Inventory;
using Manager.Global;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using OWPData.Structs;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Static;

namespace SharedUI.IGUI
{
    public class SlotsIGUIController : MonoBehaviour, MMEventListener<MMInventoryEvent>,
        MMEventListener<EquipmentUIEvent>
    {
        public const int FirstPersonSlotsTypeIndex = 0;
        public const int DirigibleSlotsTypeIndex = 1;
        [SerializeField] CustomDropdown slotsTypeDropdown;

        [Header("Item Slots First Person")] [SerializeField]
        IGUIEquipItemSlot handsItemSlot;

        [SerializeField] MMFeedbacks unequipFeedbacks;


        [SerializeField] Color defaultColor;
        [SerializeField] IGUIEquipItemSlot backItemSlot;

        [SerializeField] IGUIEquipItemSlot leftHandItemSlot;

        [SerializeField] IGUIEquipItemSlot equippedAbilityWrapperSlot;

        [FormerlySerializedAs("dirigibleLeftItemSlot")] [Header("Item Slots Dirigible")] [SerializeField]
        IGUIEquipItemSlot dirigibleMainScannerSlot;


        [Header("Canvas Groups")] [SerializeField]
        CanvasGroup firstPersonSlotsCanvasGroup;

        [SerializeField] CanvasGroup dirigibleSlotsCanvasGroup;

        InventoryItem _backItem;

        InventoryItem _dirigibleMainScannerItem;
        InventoryItem _lHandItem;

        InventoryItem _rHandItem;


        void Start()
        {
            WireUnequipButtons();
            Refresh();
        }

        void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            this.MMEventStartListening<EquipmentUIEvent>();
            if (slotsTypeDropdown != null)
                slotsTypeDropdown.onValueChanged.AddListener(OnSlotsTypeChanged);
        }

        void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
            this.MMEventStopListening<EquipmentUIEvent>();
            if (slotsTypeDropdown != null)
                slotsTypeDropdown.onValueChanged.RemoveListener(OnSlotsTypeChanged);
        }
        public void OnMMEvent(EquipmentUIEvent eventType)
        {
            if (eventType.Type == EquipmentUIEventType.UnequippedAbility) UnequipSlot(equippedAbilityWrapperSlot);
        }


        public void OnMMEvent(MMInventoryEvent e)
        {
            Refresh();
        }

        void WireUnequipButtons()
        {
            Wire(handsItemSlot);
            Wire(backItemSlot);
            Wire(leftHandItemSlot);
            Wire(dirigibleMainScannerSlot);
            Wire(equippedAbilityWrapperSlot);
        }

        void Wire(IGUIEquipItemSlot slot)
        {
            if (slot == null || slot.unequipButton == null) return;
            slot.unequipButton.onClick.RemoveAllListeners();
            slot.unequipButton.onClick.AddListener(() => { UnequipSlot(slot); });
        }

        void UnequipSlot(IGUIEquipItemSlot slot)
        {
            if (slot == null || slot.inventory == null || slot.inventory.Content == null ||
                slot.inventory.Content.Length == 0)
                return;

            var equipped = slot.inventory.Content[0];
            if (InventoryItem.IsNull(equipped))
                return;

            var global = GlobalInventoryManager.Instance;
            var playerInv = global?.playerInventory;
            if (playerInv == null)
            {
                Debug.LogError("Unequip failed: player inventory not available.");
                return;
            }

            if (playerInv.Content == null || playerInv.Content.Length == 0)
            {
                Debug.LogError("Unequip failed: player inventory is empty.");
                return;
            }

            var item = playerInv.Content[0];

            unequipFeedbacks?.PlayFeedbacks();
            slot.inventory.UnEquipItem(equipped, 0);


            // 3) Refresh so icon clears & button disables
            Refresh();
        }

        public void Refresh()
        {
            if (slotsTypeDropdown == null || handsItemSlot == null || backItemSlot == null)
            {
                Debug.LogWarning("SlotsIGUIController: Missing references in the inspector.");
                return;
            }

            if (handsItemSlot.inventory == null || backItemSlot.inventory == null)
            {
                Debug.LogWarning("SlotsIGUIController: Inventory references are not set.");
                return;
            }

            if (leftHandItemSlot.inventory == null)
            {
                Debug.LogWarning("SlotsIGUIController: Left hand inventory reference is not set.");
                return;
            }


            if (equippedAbilityWrapperSlot.inventory == null)
            {
                Debug.LogWarning("SlotsIGUIController: Equipped ability wrapper inventory reference is not set.");
                return;
            }

            // HANDS
            var hand = handsItemSlot.inventory.Content.Length > 0 ? handsItemSlot.inventory.Content[0] : null;
            var hasHand = hand != null && !InventoryItem.IsNull(hand);
            handsItemSlot.itemImage.sprite = hasHand ? hand.GetDisplayIcon() : null;
            handsItemSlot.itemImage.color = hasHand ? defaultColor : new Color(1, 1, 1, 0);
            if (handsItemSlot.unequipButton != null) handsItemSlot.unequipButton.Interactable(hasHand);
            // handsItemSlot.unequipButton.gameObject.SetActive(hasHand);
            // BACK
            var back = backItemSlot.inventory.Content.Length > 0 ? backItemSlot.inventory.Content[0] : null;
            var hasBack = back != null && !InventoryItem.IsNull(back);
            backItemSlot.itemImage.sprite = hasBack ? back.GetDisplayIcon() : null;
            backItemSlot.itemImage.color = hasBack ? defaultColor : new Color(1, 1, 1, 0);
            if (backItemSlot.unequipButton != null)
            {
                backItemSlot.unequipButton.Interactable(hasBack);
                backItemSlot.unequipButton.gameObject.SetActive(hasBack);
            }

            // LEFT HAND
            var leftHand = leftHandItemSlot.inventory.Content.Length > 0 ? leftHandItemSlot.inventory.Content[0] : null;
            var hasLeft = leftHand != null && !InventoryItem.IsNull(leftHand);
            leftHandItemSlot.itemImage.sprite = hasLeft ? leftHand.GetDisplayIcon() : null;
            leftHandItemSlot.itemImage.color = hasLeft ? defaultColor : new Color(1, 1, 1, 0);
            if (leftHandItemSlot.unequipButton != null) leftHandItemSlot.unequipButton.Interactable(hasLeft);
            // leftHandItemSlot.unequipButton.gameObject.SetActive(hasLeft);

            // EQUIPPED ABILITY WRAPPER
            var equippedAbilityWrapper = equippedAbilityWrapperSlot.inventory.Content.Length > 0
                ? equippedAbilityWrapperSlot.inventory.Content[0]
                : null;

            var hasEquippedAbilityWrapper =
                equippedAbilityWrapper != null && !InventoryItem.IsNull(equippedAbilityWrapper);

            equippedAbilityWrapperSlot.itemImage.sprite =
                hasEquippedAbilityWrapper ? equippedAbilityWrapper.GetDisplayIcon() : null;

            equippedAbilityWrapperSlot.itemImage.color =
                hasEquippedAbilityWrapper ? defaultColor : new Color(1, 1, 1, 0);

            if (equippedAbilityWrapperSlot.unequipButton != null)
                equippedAbilityWrapperSlot.unequipButton.Interactable(hasEquippedAbilityWrapper);


            // dirigibleMainScannerSlot.unequipButton.gameObject.SetActive(hasDirigibleMainScanner);
        }


        /// <summary>
        ///     Removes the specified item from all known inventories (player, equipment, back).
        /// </summary>
        void RemoveItemFromAllInventories(InventoryItem item)
        {
            var global = GlobalInventoryManager.Instance;
            if (global == null || InventoryItem.IsNull(item)) return;

            TryRemoveByRef(global.playerInventory, item);
            TryRemoveByRef(global.equipmentInventory, item);
            TryRemoveByRef(global.lEquipmentInventory, item);
            TryRemoveByRef(global.backEquipmentInventory, item);
            // TryRemoveByRef(global.dirigibleInventory, item);
            // TryRemoveByRef(global.dirigibleScannerSlot, item);
            TryRemoveByRef(global.abilitiesBankInventory, item);
        }


        void TryRemoveByRef(MoreMountains.InventoryEngine.Inventory inv, InventoryItem item)
        {
            if (inv == null) return;

            for (var i = 0; i < inv.Content.Length; i++)
            {
                var c = inv.Content[i];
                if (!InventoryItem.IsNull(c))
                    // Prefer reference match to avoid nuking *other* stacks with same ItemID
                    if (ReferenceEquals(c, item))
                    {
                        inv.RemoveItem(i, item.Quantity);
                        return;
                    }
            }

            // Fallback: remove by ID if exact reference wasn't found (optional)
            // inv.RemoveItemByID(item.ItemID, item.Quantity);
        }

        void OnSlotsTypeChanged(int arg0)
        {
            var currentMode = GameStateManager.Instance.CurrentMode;
            if (currentMode == GameMode.FirstPerson && arg0 == 1)
            {
                ShowPlayerSlots();
                AlertEvent.Trigger(
                    AlertReason.TooFarFromDirigible,
                    "You cannot access the Dirigible slots while not in Dirigible.",
                    "Not In Dirigible");

                return;
            }

            switch (arg0)
            {
                case FirstPersonSlotsTypeIndex:
                    ShowPlayerSlots();
                    break;
                case DirigibleSlotsTypeIndex:
                    ShowDirigibleSlots();
                    break;
                default:
                    Debug.LogError("Invalid slots type selected.");
                    break;
            }
        }

        public void ShowPlayerSlots()
        {
            // Show First Person
            firstPersonSlotsCanvasGroup.alpha = 1;
            firstPersonSlotsCanvasGroup.interactable = true;
            firstPersonSlotsCanvasGroup.blocksRaycasts = true;

            // Hide Dirigible and All others
            dirigibleSlotsCanvasGroup.alpha = 0;
            dirigibleSlotsCanvasGroup.interactable = false;
            dirigibleSlotsCanvasGroup.blocksRaycasts = false;

            slotsTypeDropdown.SetDropdownIndex(FirstPersonSlotsTypeIndex);
        }

        public void ShowDirigibleSlots()
        {
            // Show Dirigible
            dirigibleSlotsCanvasGroup.alpha = 1;
            dirigibleSlotsCanvasGroup.interactable = true;
            dirigibleSlotsCanvasGroup.blocksRaycasts = true;

            // Hide First Person and All others
            firstPersonSlotsCanvasGroup.alpha = 0;
            firstPersonSlotsCanvasGroup.interactable = false;
            firstPersonSlotsCanvasGroup.blocksRaycasts = false;

            slotsTypeDropdown.SetDropdownIndex(DirigibleSlotsTypeIndex);
        }
    }
}
