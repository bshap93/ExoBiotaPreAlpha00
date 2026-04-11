using System;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.UI;
using Inventory;
using Manager.Global;
using Manager.UI;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using OWPData.Structs;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities.Static;
using ItemPicker = LevelConstruct.Interactable.ItemInteractables.ItemPicker.ItemPicker;

namespace SharedUI.IGUI
{
    public class InventoryItemIGUIListElement : MonoBehaviour
    {
        public enum ItemType
        {
            Equippable,
            Consumable,
            UseableButNotConsumable,
            Other
        }


        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text itemNameText;
        [SerializeField] ButtonManager infoButton;
        [SerializeField] TMP_Text quantityText;

        [FormerlySerializedAs("useOrEquipButton")] [SerializeField]
        ButtonManager equipButton;
        [SerializeField] ButtonManager useButton;

        [SerializeField] ButtonManager placeButton;

        [SerializeField] ButtonManager moveButton;

        [SerializeField] ButtonManager hotbarButton;

        [SerializeField] SlotsIGUIController slotsIGUIController;

        [SerializeField] MMFeedbacks placeObjectFeedbacks;

        [Header("Hotbar Button Text")] [SerializeField]
        string addToHotbarText = "Add to Hotbar";
        [SerializeField] string removeFromHotbarText = "Remove from Hotbar";

        InventoryIGUIController.InventoryItemViewOptions _inventoryItemViewOptions;
        bool _isInHotbar;

        MyBaseItem _item;
        ItemType _itemType;
        int _sourceIndex;

        MoreMountains.InventoryEngine.Inventory _sourceInventory;


        public void Initialize(MoreMountains.InventoryEngine.Inventory source, int index,
            InventoryIGUIController.InventoryItemViewOptions options, int totalQuantityOverride = -1)
        {
            _sourceInventory = source;
            _sourceIndex = index;
            _item = source.Content[index] as MyBaseItem;

            _inventoryItemViewOptions = options;


            if (_item == null)
            {
                Debug.LogError($"Item at index {index} in inventory {source.name} is not a MyBaseItem.");
                return;
            }

            itemImage.sprite = _item.GetDisplayIcon();
            itemNameText.text = _item.GetDisplayName();

            var displayQty = totalQuantityOverride > 0 ? totalQuantityOverride : _item.Quantity;
            quantityText.text = $"x{displayQty}";
            quantityText.gameObject.SetActive(true);


            infoButton.onClick.AddListener(ShowItemInfo);

            equipButton.onClick.RemoveAllListeners();

            placeButton.onClick.RemoveAllListeners();


            var currentMode = GameStateManager.Instance.CurrentMode;
            if (_item.Equippable)
            {
                equipButton.onClick.AddListener(EquipViaMM);
                useButton.gameObject.SetActive(false);

                if (currentMode == GameMode.FirstPerson &&
                    _item.equippableContext == MyBaseItem.EquippableCtx.FirstPerson)
                {
                }
                else if ((currentMode == GameMode.DirigibleFlight &&
                          _item.equippableContext == MyBaseItem.EquippableCtx.Dirigible) ||
                         (currentMode == GameMode.Overview &&
                          _item.equippableContext == MyBaseItem.EquippableCtx.Dirigible))
                {
                }
                else
                {
                    equipButton.gameObject.SetActive(false);
                }
            }
            else
            {
                equipButton.gameObject.SetActive(false);
            }

            if (currentMode == GameMode.DirigibleFlight || currentMode == GameMode.Overview)
            {
                moveButton.onClick.AddListener(MoveItem);
                moveButton.gameObject.SetActive(true);
            }
            else
            {
                moveButton.gameObject.SetActive(false);
            }

            if (currentMode == GameMode.FirstPerson)
            {
                if (hotbarButton != null) SetupHotbarButton();
            }
            else
            {
                if (hotbarButton != null) hotbarButton.gameObject.SetActive(false);
            }

            if (_item.Usable && !_item.Consumable && !_item.Equippable)
                useButton.onClick.AddListener(UseNonConsumable);
            else if (_item.Usable && _item.Consumable && !_item.Equippable)
                useButton.onClick.AddListener(UseConsumable);
            else useButton.gameObject.SetActive(false);

            // if (_item.isQuestItem) placeButton.gameObject.SetActive(false);

            SetPlaceButtonActiveIf();
        }

        void SetupHotbarButton()
        {
            if (hotbarButton == null) return;

            // Check if item can be added to hotbar (tools or consumables only)
            var inventoryManager = GlobalInventoryManager.Instance;
            if (inventoryManager == null)
            {
                hotbarButton.gameObject.SetActive(false);
                return;
            }

            var isTool = inventoryManager.IsItemIDaType<RightHandEquippableTool>(_item.ItemID);
            var isConsumable = inventoryManager.IsItemIDaConsumableEffectItem(_item.ItemID);

            if (!isTool && !isConsumable)
            {
                // Item cannot be added to hotbar
                hotbarButton.gameObject.SetActive(false);
                return;
            }

            // Item can be added to hotbar
            hotbarButton.gameObject.SetActive(true);
            hotbarButton.onClick.RemoveAllListeners();
            hotbarButton.onClick.AddListener(ToggleHotbar);

            // Update button appearance based on whether item is in hotbar
            UpdateHotbarButtonState();
        }

        void UpdateHotbarButtonState()
        {
            if (hotbarButton == null || _item == null) return;

            var hotbarManager = HotbarManager.Instance;
            if (hotbarManager == null)
                _isInHotbar = false;
            else
                _isInHotbar = hotbarManager.IsItemInHotbar(_item.ItemID);

            // Update button text
            var buttonText = _isInHotbar ? removeFromHotbarText : addToHotbarText;
            hotbarButton.buttonText = buttonText;

            // Update the text in all text components
            if (hotbarButton.normalText != null) hotbarButton.normalText.text = buttonText;
            if (hotbarButton.highlightedText != null) hotbarButton.highlightedText.text = buttonText;
            if (hotbarButton.disabledText != null) hotbarButton.disabledText.text = buttonText;

            // Optional: Change button color/appearance
            // You could add logic here to change the button's color or icon
            // For example, make it a different color when item is in hotbar
        }

        void ToggleHotbar()
        {
            if (_item == null) return;

            var hotbarManager = HotbarManager.Instance;
            if (hotbarManager == null)
            {
                Debug.LogWarning("[InventoryItemIGUIListElement] HotbarManager.Instance is null!");
                return;
            }

            var itemID = _item.ItemID;
            var indexInInventory = _sourceIndex;

            if (_isInHotbar)
                // Remove from hotbar
                HotbarEvent.Trigger(
                    HotbarEvent.HotbarEventType.RemoveFromHotbar, itemID, indexInInventory);
            else
                // Add to hotbar
                HotbarEvent.Trigger(
                    HotbarEvent.HotbarEventType.AddToHotbar, itemID, indexInInventory);

            // Update button state after a short delay to allow the hotbar to update
            Invoke(nameof(UpdateHotbarButtonState), 0.1f);
        }

        // Keep the old method for backwards compatibility if needed elsewhere
        void AddToHotbar()
        {
            ToggleHotbar();
        }

        void SetPlaceButtonActiveIf()
        {
            if (_inventoryItemViewOptions is { disablePlaceButton: true })
            {
                placeButton.gameObject.SetActive(false);
                return;
            }

            if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson &&
                _item != null &&
                !_item.isQuestItem)
            {
                placeButton.onClick.AddListener(PlaceItem);
                placeButton.gameObject.SetActive(true);
            }
            else
            {
                placeButton.gameObject.SetActive(false);
            }
        }

        void MoveItem()
        {
            if (_item == null || _sourceInventory == null) return;

            if (_sourceInventory.name == "PlayerMainInventory")
            {
                Debug.Log("Moving item to DirigibleInventory");
                // var dirigibleInventory = GlobalInventoryManager.Instance.dirigibleInventory;
                // var newWeight = GlobalInventoryManager.Instance.GetTotalWeightInDirigible();

                // if (dirigibleInventory == null)
                // {
                //     Debug.LogError("No 'DirigibleInventory' for Player1 found.");
                //     return;
                // }

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.StartMove);

                AlertEvent.Trigger(
                    AlertReason.ItemMoved,
                    $"Moved item '{_item.ItemName}' to dirigible Inventory.", "Item Moved");


                // Make the move
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null, _sourceInventory.name
                    , _item, 1, _sourceIndex,
                    "Player1");

                // MMInventoryEvent.Trigger(
                //     MMInventoryEventType.Pick, null, dirigibleInventory.name, _item, 1, -1,
                //     "Player1");

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.FinishMove);
            }
            else if (_sourceInventory.name == "DirigibleInventory")
            {
                Debug.Log("Moving item to PlayerMainInventory");
                var playerInventory = GlobalInventoryManager.Instance.playerInventory;
                if (playerInventory == null)
                {
                    Debug.LogError("No 'PlayerMainInventory' for Player1 found.");
                    return;
                }

                var pInvCurrentWeight = GlobalInventoryManager.Instance.GetWeightOfInventory(playerInventory);
                var pInvMaxWeight = GlobalInventoryManager.Instance.GetMaxWeightOfPlayerCarry();
                var itemWeight = _item.weight * _item.Quantity;

                if (pInvCurrentWeight + itemWeight > pInvMaxWeight)
                {
                    AlertEvent.Trigger(
                        AlertReason.InventoryFull,
                        $"Cannot move item '{_item.ItemName}'. Player inventory weight limit exceeded.",
                        "Inventory Full");

                    return;
                }

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.StartMove);

                AlertEvent.Trigger(
                    AlertReason.ItemMoved,
                    $"Moved item '{_item.ItemName}' to Player Inventory.", "Item Moved");

                // Make the move
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null, _sourceInventory.name
                    , _item, 1, _sourceIndex,
                    "Player1");

                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Pick, null, playerInventory.name, _item, 1, -1,
                    "Player1");

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.FinishMove);
            }
        }

        void PlaceItem()
        {
            var propsItemHold = FindFirstObjectByType<PlayerPropPickup>();
            if (propsItemHold == null)
            {
                Debug.LogError("No PlayerPropPickup found in scene.");
                return;
            }

            if (_item.isQuestItem)
            {
                AlertEvent.Trigger(
                    AlertReason.CannotPlaceQuestItem, "Cannot remove a quest item by placing it.",
                    "Cannot Remove Quest Item");

                return;
            }


            if (propsItemHold.heldRb == null)
            {
                // if (propsItemHold.AreBothHandsOccupied())
                //     UnequipViaMM();
                // PlayerInteraction.Instance.RightHandEquipment.UnequipTool();

                var playerInventory = GlobalInventoryManager.Instance.playerInventory;
                var prefab = _item.Prefab;
                playerInventory?.RemoveItemByID(_item.ItemID, 1);
                var holdPoint = propsItemHold.holdPoint;
                var instance = Instantiate(prefab, holdPoint.position, holdPoint.rotation);
                placeObjectFeedbacks?.PlayFeedbacks();
                var itemPicker = instance.GetComponentInChildren<ItemPicker>();
                itemPicker.OnPlacedByPlayer();

                // var statefulItemPicker = instance.GetComponent<IStatefulItemPicker>();
                // if (statefulItemPicker != null) statefulItemPicker.SetStateToDefault();
                itemPicker.uniqueID = Guid.NewGuid().ToString();
                propsItemHold.SetItem(instance);
                MyUIEvent.Trigger(UIType.InGameUI, UIActionType.Close);
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, 67, "BlockAllNewRequests");
            }
            else
            {
                AlertEvent.Trigger(
                    AlertReason.HoldingItemAlready, "Cannot place item, already holding one.", "Holding an Item");
            }
        }

        void DropItem()
        {
            if (_item == null || _sourceInventory == null) return;
            // Remove one quantity of the item from the source inventory
            _sourceInventory.DropItem(_item, _sourceIndex);
            Debug.Log($"Dropped item: {_item.ItemName} from {_sourceInventory.name}[{_sourceIndex}]");
        }


        void EquipViaMM()
        {
            if (_item == null || _sourceInventory == null) return;

            var playerEquipment = PlayerEquipment.Instance;
            if (_item is BioticAbilityToolWrapper && playerEquipment != null)
                if (playerEquipment.IsRanged)
                {
                    AlertEvent.Trigger(
                        AlertReason.RangedWeaponInUse,
                        "Cannot equip biotic ability while a ranged weapon is equipped.",
                        "Cannot Equip Ability");

                    return;
                }

            // Make sure target equipment inventory exists & has room
            var equipInv = MoreMountains.InventoryEngine.Inventory.FindInventory("EquippedItemInventory", "Player1");
            if (equipInv == null)
            {
                Debug.LogError("No 'EquippedItemInventory' for Player1 found.");
                return;
            }

            // Ensure capacity >= 1
            if (equipInv.Content == null || equipInv.Content.Length < 1)
            {
                Debug.LogError(
                    $"Equipment inventory '{equipInv.name}' has size {equipInv.Content?.Length ?? 0}. Set it to >= 1.");

                return;
            }

            // Smart hotbar integration for right-hand tools
            var inventoryManager = GlobalInventoryManager.Instance;
            if (inventoryManager != null && inventoryManager.IsItemIDaType<RightHandEquippableTool>(_item.ItemID))
            {
                var hotbarManager = HotbarManager.Instance;
                if (hotbarManager != null) HandleSmartToolEquip(hotbarManager);
            }

            // Trigger the actual equip
            MMInventoryEvent.Trigger(
                MMInventoryEventType.EquipRequest, null, _item.TargetInventoryName, _item, 1, _sourceIndex, "Player1");
        }

        void HandleSmartToolEquip(HotbarManager hotbarManager)
        {
            var itemID = _item.ItemID;
            var inventoryIndex = _sourceIndex;

            // Case 1: Tool is already in the hotbar
            var existingSlotIndex = hotbarManager.GetToolSlotIndex(itemID);
            if (existingSlotIndex >= 0)
            {
                // Switch to that hotbar slot
                hotbarManager.SetCurrentToolSlotIndex(existingSlotIndex);
                Debug.Log(
                    $"[SmartEquip] Tool {_item.ItemName} already in hotbar at slot {existingSlotIndex}, switching to it.");

                return;
            }

            // Case 2: Tool is not in hotbar, check for empty slot
            var emptySlotIndex = hotbarManager.GetFirstEmptyToolSlot();
            if (emptySlotIndex >= 0)
            {
                // Add to empty slot
                HotbarEvent.Trigger(HotbarEvent.HotbarEventType.AddToHotbar, itemID, inventoryIndex);
                hotbarManager.SetCurrentToolSlotIndex(emptySlotIndex);
                return;
            }

            // Case 3: Hotbar is full, replace currently equipped tool
            var currentSlotIndex = hotbarManager.GetCurrentToolSlotIndex();

            // If current slot is 0 (empty hands), replace slot 1 instead
            if (currentSlotIndex == 0) currentSlotIndex = 1;

            // Replace the tool in the current slot
            hotbarManager.ReplaceToolInSlot(currentSlotIndex, itemID, inventoryIndex);
            Debug.Log($"[SmartEquip] Replaced tool in hotbar slot {currentSlotIndex} with {_item.ItemName}.");
        }


        void UseConsumable()
        {
            if (_item == null) return;

            // ask the item to use itself for Player1.
            _item.Use("Player1");

            // Optionally, remove the item from the inventory after use
            _sourceInventory.RemoveItem(_sourceIndex, _item.Quantity);
        }

        void UseNonConsumable()
        {
            if (_item == null) return;

            // ask the item to use itself for Player1.
            _item.Use("Player1");

            Debug.Log($"Used non-consumable item: {_item.ItemName}");
        }


        void ShowItemInfo()
        {
            InventoryEvent.Trigger(InventoryEventType.ShowItem, null, _item);
        }
    }
}
