using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI
{
    public class IGUIEquipItemSlot : MonoBehaviour
    {
        public string slotName;
        public Image itemImage;

        public TMP_Text label;


        public MoreMountains.InventoryEngine.Inventory inventory;

        [FormerlySerializedAs("buttonManager")] [SerializeField]
        public ButtonManager unequipButton;

        private void OnEnable()
        {
            label.text = slotName;
        }
    }
}