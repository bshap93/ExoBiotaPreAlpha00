using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Shop
{
    public class BuySellItemsElementUI : MonoBehaviour
    {
        // UI Elements
        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text itemNameText;
        [SerializeField] TMP_Text itemPriceText;
        [SerializeField] TMP_Text itemQuantityText;
        [SerializeField] TMP_Text itemWeightText;
        [SerializeField] ButtonManager buyButton;
        [SerializeField] ButtonManager sellButton;
        [SerializeField] ButtonManager infoButton;

        MyBaseItem _currentItem;
        int _currentQuantity;
        string _inventoryId;
        bool _isSell;
        string _npcId;

        public void Initialize(MyBaseItem item, int quantity, bool sell, string npcId,
            string itemLocationInventoryName)
        {
            itemImage.sprite = item.Icon;
            itemNameText.text = item.ItemName;
            var price = sell ? item.normalSellPrice : item.normalBuyPrice;
            itemPriceText.text = $"{price}";
            itemQuantityText.text = $"{quantity}";
            itemWeightText.text = $"{item.weight}";
            buyButton.gameObject.SetActive(!sell);
            sellButton.gameObject.SetActive(sell);

            _currentItem = item;
            _currentQuantity = quantity;
            _isSell = sell;
            _inventoryId = itemLocationInventoryName;

            if (!sell)
            {
                buyButton.gameObject.SetActive(true);
                buyButton.onClick.RemoveAllListeners();
                sellButton.gameObject.SetActive(false);
                sellButton.onClick.RemoveAllListeners();

                buyButton.onClick.AddListener(Buy);

                infoButton.onClick.RemoveAllListeners();
                infoButton.onClick.AddListener(ShowItemInfo);
            }
            else
            {
                sellButton.gameObject.SetActive(true);
                sellButton.onClick.RemoveAllListeners();
                buyButton.gameObject.SetActive(false);
                buyButton.onClick.RemoveAllListeners();

                sellButton.onClick.AddListener(Sell);

                infoButton.onClick.RemoveAllListeners();
                infoButton.onClick.AddListener(ShowItemInfo);
            }
        }

        public void Buy()
        {
            ShoppingEvent.Trigger(_npcId, ShoppingEventType.BoughtItem, _currentQuantity, _currentItem);
        }

        public void Sell()
        {
            ShoppingEvent.Trigger(_npcId, ShoppingEventType.SoldItem, _currentQuantity, _currentItem, _inventoryId);
        }

        void ShowItemInfo()
        {
            InventoryEvent.Trigger(InventoryEventType.ShowItem, null, _currentItem);
        }
    }
}
