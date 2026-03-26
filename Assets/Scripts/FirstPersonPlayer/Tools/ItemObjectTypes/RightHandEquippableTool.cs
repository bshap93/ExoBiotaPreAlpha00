using System;
using CompassNavigatorPro;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "RightHandEquippableTool",
        menuName = "Scriptable Objects/Items/RightHandEquippableTool",
        order = 0)]
    [Serializable]
    public class RightHandEquippableTool : BaseTool
    {
        public ScanProfile scannerProfile;

        // [SerializeField] float baseStaminaConsumedPerUse = 3f;

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

        [FormerlySerializedAs("ToolReach")] public float toolReach;
        public Vector3 GetToolAttackOrigin()
        {
            return Vector3.zero;
        }
    }
}
