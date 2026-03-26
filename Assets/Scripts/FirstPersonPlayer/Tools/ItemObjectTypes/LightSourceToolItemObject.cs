using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "LightSourceToolItemObject",
        menuName = "Scriptable Objects/Items/LightSourceToolItemObject", order = 0)]
    public class LightSourceToolItemObject : BaseTool
    {
        public override bool Equip(string playerID)
        {
            MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped,
                null, // Slot is not used in this context
                "LEquipmentInventory", // Assuming this is the inventory name
                this, // The item being equipped
                1, // Quantity, assuming 1 for equipping
                -1, // Index, not used here
                playerID);

            return base.Equip(playerID);
        }
    }
}