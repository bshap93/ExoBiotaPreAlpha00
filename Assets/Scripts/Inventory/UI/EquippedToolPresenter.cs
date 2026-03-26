using System.Collections;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class EquippedToolPresenter : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public enum EquipSlotType
        {
            RightHandedTools,
            BioticAbilities
        }

        public enum InventoryAction
        {
            Equip,
            Unequip,
            Change,
            Ready
        }

        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField] EquipSlotType equipSlotType;

        [SerializeField] Color defaultColor;

        [SerializeField] MoreMountains.InventoryEngine.Inventory equippedToolInventory;
        [SerializeField] Image equippedToolImage;
        [SerializeField] TMP_Text equippedToolAbbreviatedNameTxt;

        [FormerlySerializedAs("_equippedTool")] [SerializeField]
        InventoryItem equippedTool;

        Coroutine _lateInit;

        void Start()
        {
            Refresh(InventoryAction.Ready);
        }


        void OnEnable()
        {
            this.MMEventStartListening();

            // Defer a one-time rebuild until the inventory has real content
            if (_lateInit != null) StopCoroutine(_lateInit);
            _lateInit = StartCoroutine(RefreshWhenInvReady());
        }

        void OnDisable()
        {
            this.MMEventStopListening();

            if (_lateInit != null)
            {
                StopCoroutine(_lateInit);
                _lateInit = null;
            }
        }

        public void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (inventoryEvent.TargetInventoryName != equippedToolInventory?.name ||
                inventoryEvent.PlayerID != equippedToolInventory?.PlayerID) return;

            var item = inventoryEvent.EventItem;

            switch (equipSlotType)
            {
                case EquipSlotType.RightHandedTools:
                    if (item == null || !(item is RightHandEquippableTool)) return;
                    break;
                case EquipSlotType.BioticAbilities:
                    if (item == null || !(item is BioticAbilityToolWrapper)) return;
                    break;
            }

            if (inventoryEvent.InventoryEventType == MMInventoryEventType.ItemUnEquipped)
                Refresh(InventoryAction.Unequip);
            else if (inventoryEvent.InventoryEventType == MMInventoryEventType.ItemEquipped)
                Refresh(InventoryAction.Equip);
            else if (inventoryEvent.InventoryEventType == MMInventoryEventType.ContentChanged)
                Refresh(InventoryAction.Change);

            // Refresh();
        }

        IEnumerator RefreshWhenInvReady()
        {
            // Short, bounded wait to avoid racing the loader/equip on respawn
            var timeoutAt = Time.realtimeSinceStartup + 2f;
            while (equippedToolInventory == null || equippedToolInventory.Content == null ||
                   !HasRealItem(equippedToolInventory))
            {
                if (Time.realtimeSinceStartup > timeoutAt) break;
                yield return null;
            }

            Refresh(InventoryAction.Ready);
            _lateInit = null;
        }


        static bool HasRealItem(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv == null || inv.Content == null) return false;
            foreach (var it in inv.Content)
                if (!InventoryItem.IsNull(it) && it.Quantity > 0)
                    return true;

            return false;
        }

        void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        void Refresh(InventoryAction action)
        {
            var current = equippedToolInventory.Content.Length > 0
                ? equippedToolInventory.Content[0]
                : null;

            if (current == null)
            {
                equippedToolImage.sprite = null;
                equippedToolImage.color = new Color(1, 1, 1, 0);
                equippedToolAbbreviatedNameTxt.text = "None";
                Hide();
                return;
            }

            if (action == InventoryAction.Equip)
            {
                equippedToolImage.color = defaultColor;
                equippedToolImage.sprite = current.Icon;
                equippedToolAbbreviatedNameTxt.text = current.ItemName;
                Show();
            }
            else if (action == InventoryAction.Unequip)
            {
                equippedToolImage.sprite = null;
                equippedToolImage.color = new Color(1, 1, 1, 0);
                equippedToolAbbreviatedNameTxt.text = "None";
                Hide();
            }
            else if (action == InventoryAction.Change)
            {
                equippedToolImage.sprite = current?.Icon;
                equippedToolAbbreviatedNameTxt.text = current != null ? current.ItemName : "None";

                if (current != null)
                    Show();
                else
                    Hide();
            }
            else if (action == InventoryAction.Ready)
            {
                if (current == null)
                {
                    equippedToolImage.color = new Color(1, 1, 1, 0);
                    equippedToolAbbreviatedNameTxt.text = "None";
                    Hide();
                }
                else
                {
                    equippedToolImage.sprite = current?.Icon;
                    equippedToolAbbreviatedNameTxt.text = current.ItemName;
                    Show();
                }
            }
        }

        public void SetEquippedTool(InventoryItem tool)
        {
            equippedTool = tool;
            // Update the UI or perform any necessary actions with the equipped tool
            Debug.Log($"Equipped tool set to: {equippedTool?.name}");

            if (equippedTool != null && equippedToolImage != null)
            {
                equippedToolImage.sprite = equippedTool.Icon; // Assuming InventoryItem has an ItemImage property
                equippedToolImage.color = defaultColor; // Reset color to white when a tool is equipped
            }
            else if (equippedToolImage != null)
            {
                equippedToolImage.sprite = null; // Clear the image if no tool is equipped
            }
        }
    }
}
