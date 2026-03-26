using MoreMountains.InventoryEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Static;

namespace CustomAssets.AssetExtensions.InventoryEngine.PickupDisplayer
{
    public class PickupDisplayItem : MonoBehaviour
    {
        [SerializeField] Image Icon;
        [SerializeField] TMP_Text Name;
        [SerializeField] TMP_Text Quantity;

        public void Display(InventoryItem item, int quantity, Sprite iconOverride)
        {
            Icon.sprite = iconOverride ?? item.GetDisplayIcon();
            Name.text = $"{item.ItemName} Sample";
            Quantity.text = quantity.ToString();
        }

        public void AddQuantity(int quantity)
        {
            Quantity.text = (int.Parse(Quantity.text) + quantity).ToString();
        }
    }
}
