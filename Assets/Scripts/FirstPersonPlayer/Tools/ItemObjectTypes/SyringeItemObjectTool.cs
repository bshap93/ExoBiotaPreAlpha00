using System;
using FirstPersonPlayer.ScriptableObjects.BioticAbility;
using Helpers.Events.Feedback;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "SyringeItemObjectTool", menuName = "Scriptable Objects/Items/SyringeItemObjectTool")]
    [Serializable]
    public class SyringeItemObjectTool : BaseTool
    {
        public BioticAbilityToolWrapper bioticAbilityInvItem;
        [SerializeField] bool equipToUse;
        public BioticAbility bioticAbility => bioticAbilityInvItem.bioticAbility;
        public override bool Equip(string playerID)
        {
            if (!equipToUse)
                return base.Equip(playerID);

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

        public override bool Use(string playerID)
        {
            if (equipToUse) return false;
            base.Use(playerID);
            MMInventoryEvent.Trigger(
                MMInventoryEventType.Pick, null, bioticAbilityInvItem.TargetInventoryName,
                bioticAbilityInvItem, 1, 0, playerID);

            ConsumableFeedbackEvent.Trigger(
                ConsumableFeedbackEventType.InjectableAbilityItemUsed, bioticAbility.UniqueID,
                bioticAbilityInvItem.TargetInventoryName, bioticAbilityInvItem, playerID);


            return true;
        }
    }
}
