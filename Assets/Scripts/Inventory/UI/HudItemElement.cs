using MoreMountains.InventoryEngine;
using TMPro;
using UnityEngine.UI;
using Utilities.Static;

namespace Inventory.UI
{
    public class HudItemElement : ItemElementBase
    {
        public Image icon;
        public TMP_Text quantity;

        // public override void Bind(InventoryItem item)
        // {
        //
        // }

        public override void Bind(InventoryItem item)
        {
            icon.sprite = item.GetDisplayIcon();
            quantity.text = item.Quantity.ToString();
        }
    }
}