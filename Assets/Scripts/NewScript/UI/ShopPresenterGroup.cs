using Helpers.Events;
using MoreMountains.Tools;
using SharedUI.Shop;
using UnityEngine;

namespace NewScript.UI
{
    public class ShopPresenterGroup : MonoBehaviour, MMEventListener<ShoppingEvent>
    {
        [SerializeField] GameObject sellItemsUI;
        [SerializeField] GameObject buyItemsUI;
        CanvasGroup _canvasGroup;
        void OnEnable()
        {
            this.MMEventStartListening();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(ShoppingEvent shoppingEvent)
        {
            var npcId = shoppingEvent.NpcId;
            switch (shoppingEvent.EventType)
            {
                case ShoppingEventType.StartShoppingBuy:
                    ShowBuyItemsUI(npcId);
                    break;
                case ShoppingEventType.StartShoppingSell:
                    ShowSellItemsUI(npcId);
                    break;
                case ShoppingEventType.StartShoppingSellIllegal:
                    ShowSellItemsUI(npcId, true);
                    break;
                case ShoppingEventType.StopShoppingBuy:
                    HideBuyItemsUI(npcId);
                    break;
                case ShoppingEventType.StopShoppingSell:
                    HideSellItemsUI(npcId);
                    break;
            }
        }

        void ShowSellItemsUI(string npcId, bool illegal = false)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            var sellItemsUIComponent = sellItemsUI.GetComponent<SellItemsUI>();
            if (sellItemsUIComponent != null)
                sellItemsUIComponent.Initialize(npcId, illegal);
            else Debug.LogError("SellItemsUI component not found on sellItemsUI GameObject.");

            sellItemsUI.SetActive(true);
        }

        void ShowBuyItemsUI(string npcId)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            var buyItemsUIComponent = buyItemsUI.GetComponent<BuyItemsUI>();
            if (buyItemsUIComponent != null)
                buyItemsUIComponent.Initialize(npcId);
            else Debug.LogError("BuyItemsUI component not found on buyItemsUI GameObject.");

            buyItemsUI.SetActive(true);
        }

        void HideSellItemsUI(string npcId)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            sellItemsUI.SetActive(false);
        }
        void HideBuyItemsUI(string npcId)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            buyItemsUI.SetActive(false);
        }
    }
}
