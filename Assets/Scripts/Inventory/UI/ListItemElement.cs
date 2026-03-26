using MoreMountains.InventoryEngine;
using TMPro;
using UnityEngine.UI;
using Utilities.Static;

namespace Inventory.UI
{
    public class ListItemElement : ItemElementBase
    {
        public Image icon;
        public TMP_Text nameLabel;
        public TMP_Text qtyLabel;
        public TMP_Text categoryLabel; // whatever else you need

        // public override void Bind(InventoryItem item)
        // {
        //     icon.sprite     = item.Icon;
        //     nameLabel.text  = item.ItemName;
        //     qtyLabel.text   = item.Quantity.ToString();
        //     categoryLabel.text = item.ItemClass.ToString();
        // }

        public override void Bind(InventoryItem item)
        {
            icon.sprite = item.GetDisplayIcon();
            nameLabel.text = item.GetDisplayName();
            qtyLabel.text = item.Quantity.ToString();
            categoryLabel.text = item.ItemClass.ToString();
        }
    }
}