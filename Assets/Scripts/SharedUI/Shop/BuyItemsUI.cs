using Inventory;
using Manager;
using TMPro;
using UnityEngine;

namespace SharedUI.Shop
{
    public class BuyItemsUI : MonoBehaviour
    {
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject itemElementPrefab;
        [SerializeField] TMP_Text headerText;

        string _currentNpcId;

        NPCShopStock _currentNpcStock;

        MoreMountains.InventoryEngine.Inventory _dirigibleInventory;


        public void Initialize(string npcId)
        {
            _currentNpcId = npcId;
            _currentNpcStock = ShopManager.Instance.GetNPCStock(npcId);

            foreach (Transform child in listRoot) Destroy(child.gameObject);

            AssignInventories();

            if (_dirigibleInventory == null)
            {
                Debug.LogError("Dirigible Inventory is not set.");
                return;
            }

            var itemsForSale = _currentNpcStock?.itemsForSale;

            if (itemsForSale == null || itemsForSale.Length == 0)
            {
                Debug.LogWarning($"No items found for NPC with ID: {npcId}");
                return;
            }

            for (var i = 0; i < itemsForSale.Length; i++)
            {
                var go = Instantiate(itemElementPrefab, listRoot);
                var element = go.GetComponent<BuySellItemsElementUI>();
                element.Initialize(itemsForSale[i], 1, false, npcId, null);
            }
        }

        void AssignInventories()
        {
            if (GlobalInventoryManager.Instance == null ||
                GlobalInventoryManager.Instance.dirigibleInventory == null)
            {
                Debug.LogError("GlobalInventoryManager or its inventories are not set.");
                return;
            }

            _dirigibleInventory = GlobalInventoryManager.Instance.dirigibleInventory;
        }
    }
}
