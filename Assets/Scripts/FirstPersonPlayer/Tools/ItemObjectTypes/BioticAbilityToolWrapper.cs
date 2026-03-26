using FirstPersonPlayer.ScriptableObjects.BioticAbility;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "BioticAbilityToolWrapper", menuName = "Scriptable Objects/Items/BioticAbilityToolWrapper",
        order = 0)]
    public class BioticAbilityToolWrapper : BaseTool
    {
        public BioticAbility bioticAbility;

        public override bool Equip(string playerID)
        {
            MMInventoryEvent.Trigger(
                MMInventoryEventType.ItemEquipped,
                null, // Slot is not used in this context
                "AbilityEquippedItemInv", // Assuming this is the inventory name
                this, // The item being equipped
                1, // Quantity, assuming 1 for equipping
                -1, // Index, not used here
                playerID);

            return base.Equip(playerID);
        }
    }
}
