using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Inventory;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI.Shop
{
    public class SellItemsUI : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        [SerializeField] TMP_Text headerText;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject itemElementPrefab;

        string _currentNpcId;
        MoreMountains.InventoryEngine.Inventory _dirigibleInventory;

        bool _illegal;

        MoreMountains.InventoryEngine.Inventory _playerInventory;
        void OnEnable()
        {
            this.MMEventStartListening();
            AssignInventories();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            var item = eventType.EventItem;
            var myBaseItem = item as MyBaseItem;
            if (myBaseItem != null)
            {
                if (myBaseItem.illegalSellable)
                    _illegal = true;
                else
                    _illegal = false;
            }

            if (_currentNpcId != null)
                Initialize(_currentNpcId, _illegal);
        }
        void AssignInventories()
        {
            if (GlobalInventoryManager.Instance == null ||
                GlobalInventoryManager.Instance.playerInventory == null ||
                GlobalInventoryManager.Instance.dirigibleInventory == null)
            {
                Debug.LogError("GlobalInventoryManager or its inventories are not set.");
                return;
            }

            _playerInventory = GlobalInventoryManager.Instance.playerInventory;
            _dirigibleInventory = GlobalInventoryManager.Instance.dirigibleInventory;
        }


        public void Initialize(string npcId, bool illegal = false)
        {
            // headerText.text = $"Sell Items to {npcId}";
            _currentNpcId = npcId;

            foreach (Transform child in listRoot) Destroy(child.gameObject);

            AssignInventories();

            if (_playerInventory == null || _dirigibleInventory == null)
            {
                Debug.LogError("Inventories are not set.");
                return;
            }

            var itemsToSellFromPlayerInventory = new List<MyBaseItem>();
            var itemsToSellFromDirigibleInventory = new List<MyBaseItem>();

            foreach (var item in _playerInventory.Content)
                if (item is MyBaseItem myBaseItem)
                    if (myBaseItem.legalSellable && !illegal)
                        itemsToSellFromPlayerInventory.Add(myBaseItem);
                    else if (myBaseItem.illegalSellable && illegal)
                        itemsToSellFromPlayerInventory.Add(myBaseItem);

            foreach (var item in _dirigibleInventory.Content)
                if (item is MyBaseItem myBaseItem)
                    if (myBaseItem.legalSellable && !illegal)
                        itemsToSellFromDirigibleInventory.Add(myBaseItem);
                    else if (myBaseItem.illegalSellable && illegal)
                        itemsToSellFromDirigibleInventory.Add(myBaseItem);


            for (var i = 0; i < itemsToSellFromPlayerInventory.Count; i++)
            {
                var go = Instantiate(itemElementPrefab, listRoot);
                var element = go.GetComponent<BuySellItemsElementUI>();
                element.Initialize(
                    itemsToSellFromPlayerInventory[i], 1,
                    true, npcId, _playerInventory.name);
            }

            for (var i = 0; i < itemsToSellFromDirigibleInventory.Count; i++)
            {
                var go = Instantiate(itemElementPrefab, listRoot);
                var element = go.GetComponent<BuySellItemsElementUI>();
                element.Initialize(
                    itemsToSellFromDirigibleInventory[i], 1,
                    true, npcId, _dirigibleInventory.name);
            }
        }
    }
}
