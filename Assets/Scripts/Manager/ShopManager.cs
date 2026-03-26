using System;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Interfaces;
using Helpers.YarnSpinner;
using Manager.Global;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using DialogueGameCommands = Helpers.YarnSpinner.DialogueGameCommands;

namespace Manager
{
    [Serializable]
    public class NPCShopStock
    {
        [FormerlySerializedAs("NpcId")] public string npcId;
        [FormerlySerializedAs("ItemsForSale")] public MyBaseItem[] itemsForSale;
    }

    public class ShopManager : MonoBehaviour, IDialogueGameService, MMEventListener<ShoppingEvent>
    {
        [SerializeField] MMFeedbacks buyFeedbacks;
        [SerializeField] MMFeedbacks sellFeedbacks;

        [SerializeField] string playerInventoryNameId;
        [SerializeField] string dirigibleInventoryNameId;

        [SerializeField] NPCShopStock[] npcStocks;
        CustomCommands _dialogueCustomCommands;

        DialogueGameCommands _dialogueGameCommands;
        public static ShopManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ShoppingEvent shoppingEvent)
        {
            switch (shoppingEvent.EventType)
            {
                case ShoppingEventType.SoldItem:
                    CurrencyEvent.Trigger(
                        CurrencyEventType.AddCurrency,
                        shoppingEvent.CurrentItem.normalSellPrice * shoppingEvent.CurrentQuantity);

                    // Implement selling logic here  
                    if (_dialogueGameCommands == null)
                        _dialogueGameCommands = FindFirstObjectByType<DialogueGameCommands>();

                    if (_dialogueCustomCommands == null)
                        _dialogueCustomCommands = FindFirstObjectByType<CustomCommands>();

                    if (shoppingEvent.InventoryId == playerInventoryNameId)
                        _dialogueGameCommands?.RemovePlayerItem(shoppingEvent.CurrentItem.ItemID);
                    else if (shoppingEvent.InventoryId == dirigibleInventoryNameId)
                        _dialogueGameCommands?.RemoveDirigibleItem(shoppingEvent.CurrentItem.ItemID);

                    sellFeedbacks?.PlayFeedbacks();
                    break;
                case ShoppingEventType.BoughtItem:
                    if (PlayerCurrencyManager.Instance.PlayerDollarAmount >= shoppingEvent.CurrentQuantity)
                    {
                        CurrencyEvent.Trigger(
                            CurrencyEventType.RemoveCurrency, shoppingEvent.CurrentQuantity *
                                                              shoppingEvent.CurrentItem
                                                                  .normalBuyPrice);

                        if (_dialogueGameCommands == null)
                            _dialogueGameCommands = FindFirstObjectByType<DialogueGameCommands>();

                        _dialogueCustomCommands?.GivePlayerItem(shoppingEvent.CurrentItem.ItemID);
                        buyFeedbacks?.PlayFeedbacks();
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.InsufficientFunds,
                            "You do not have enough funds to make this purchase.", "Insufficient Funds");

                        ;
                    }

                    break;
            }
        }

        public NPCShopStock GetNPCStock(string npcId)
        {
            foreach (var stock in npcStocks)
                if (stock.npcId == npcId)
                    return stock;

            Debug.LogWarning($"No stock found for NPC ID: {npcId}");
            return null;
        }
    }
}
