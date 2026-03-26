using System;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Overview.Trader.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TraderInventory", menuName = "Scriptable Objects/TraderInventory")]
    public class TraderInventoryDefinition : ScriptableObject
    {
        public List<StockItem> stock = new();

        [Serializable]
        public struct StockItem
        {
            public InventoryItem invItem; // item + default qty
            public int quantity; // trader's stock quantity
            public int buyPrice; // player buys FROM trader
            public int sellPrice; // player sells TO trader
        }
    }
}