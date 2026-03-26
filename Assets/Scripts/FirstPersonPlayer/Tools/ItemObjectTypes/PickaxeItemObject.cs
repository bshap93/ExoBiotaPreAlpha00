using System;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "PickaxeItemObject", menuName = "Scriptable Objects/Items/PickaxeItemObject",
        order = 0)]
    [Serializable]
    public class PickaxeItemObject : RightHandEquippableTool 
    {
        public override bool Equip(string playerID)
        {
            MMInventoryEvent.Trigger(
                MMInventoryEventType.ItemEquipped,
                null, // Slot is not used in this context
                "EquippedItemInventory", // Assuming this is the inventory name
                this, // The item being equipped
                1, // Quantity, assuming 1 for equipping
                -1, // Index, not used here
                playerID);


            return base.Equip(playerID);
        }

        public override bool UnEquip(string playerID)
        {
            return true;
        }

        public override bool Use(string playerID)
        {
            return true;
        }

        public override bool Drop(string playerID)
        {
            Debug.Log("Dropping Pickaxe Item Object");
            return true;
        }
    }
}
