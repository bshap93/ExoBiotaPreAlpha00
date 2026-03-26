using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Inventory;
using Inventory;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Billboard
{
    public class IGUIItemInfoPopup : MonoBehaviour, MMEventListener<InventoryEvent>, MMEventListener<MyUIEvent>,
        MMEventListener<ItemInfoUIEvent>
    {
        [Header("Canvas Group")] [SerializeField]
        CanvasGroup canvasGroup;
        [Header("Item Info Fields")] [SerializeField]
        TMP_Text itemName;
        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text itemDescription;
        [Header("Item Attributes Fields")] [SerializeField]
        TMP_Text itemWeight;
        [SerializeField] Image equippableCheckbox;
        [SerializeField] Image usableCheckbox;
        [SerializeField] Image consumableCheckbox;
        [SerializeField] Image stackableCheckbox;
        [SerializeField] TMP_Text stackableText;
        [Header("Sprites")] [SerializeField] Sprite uncheckedSprite;
        [SerializeField] Sprite checkedSprite;


        [Header("Item Reference")] public MyBaseItem currentItem;

        [Header("Buttons")] [SerializeField] ButtonManager closeButton;

        void Start()
        {
            closeButton.onClick.AddListener(Close);
            Hide();
        }

        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<InventoryEvent>();
            this.MMEventStartListening<ItemInfoUIEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<InventoryEvent>();
            this.MMEventStopListening<ItemInfoUIEvent>();
        }
        public void OnMMEvent(InventoryEvent eventType)
        {
            if (eventType.EventType == InventoryEventType.ShowItem)
                if (eventType.Item != null)
                    Open(eventType.Item);
                else
                    AlertEvent.Trigger(
                        AlertReason.InvalidAction, "Item info not able to be shown",
                        "Cannot Show Item Info");
        }
        public void OnMMEvent(ItemInfoUIEvent eventType)
        {
            if (eventType.EventType == ItemInfoUIEventType.ShowNewItemType)
            {
                var item = GlobalInventoryManager.Instance.inventoryDatabaseVariable.GetItemAsset(eventType.ItemId);
                if (item != null)
                {
                    closeButton.onClick.AddListener(PickedPreviewClose);
                    Open(item);
                }
                else
                {
                    AlertEvent.Trigger(
                        AlertReason.InvalidAction, $"Item with ID {eventType.ItemId} not found in database",
                        "Cannot Show Item Info");
                }
            }
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiActionType == UIActionType.Close)
                if (eventType.uiType == UIType.Any || eventType.uiType == UIType.InGameUI)
                    Close();
        }
        public void PickedPreviewClose()
        {
            MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
            closeButton.onClick.RemoveListener(PickedPreviewClose);
        }

        void SetCurrentItem(MyBaseItem item)
        {
            currentItem = item;

            itemName.text = item.ItemName;
            itemImage.sprite = item.Icon;
            itemDescription.text = item.Description;
            itemWeight.text = $"{item.weight:0.##} KG";
            equippableCheckbox.sprite = item.Equippable ? checkedSprite : uncheckedSprite;
            usableCheckbox.sprite = item.Usable ? checkedSprite : uncheckedSprite;
            consumableCheckbox.sprite = item.Consumable ? checkedSprite : uncheckedSprite;
            stackableCheckbox.sprite = item.MaximumStack > 1 ? checkedSprite : uncheckedSprite;
            stackableText.text = item.MaximumStack > 1 ? item.MaximumStack.ToString() : "";
        }

        void ResetFields()
        {
            itemName.text = "";
            itemImage.sprite = null;
            itemDescription.text = "";
            itemWeight.text = "";
            equippableCheckbox.sprite = uncheckedSprite;
            usableCheckbox.sprite = uncheckedSprite;
            consumableCheckbox.sprite = uncheckedSprite;
            stackableCheckbox.sprite = uncheckedSprite;
            stackableText.text = "";
        }

        void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Open(MyBaseItem item)
        {
            SetCurrentItem(item);
            Show();
        }

        void Close()
        {
            Hide();
            ResetFields();
        }
    }
}
