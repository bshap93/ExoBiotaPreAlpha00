using CompassNavigatorPro;
using Inventory;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "DirigibleMountedModule",
        menuName = "Scriptable Objects/Items/DirigibleMountedModule", order = 0)]
    public class DirigibleFrontMountedModule : MyBaseItem
    {
        public ScanProfile mntScannerProfile;
        [Header("Runtime")] public GameObject DirigibleModulePrefab; // must have an IRuntimeTool on it
        public float Cooldown;

        public override bool Equip(string playerID)
        {
            MMInventoryEvent.Trigger(
                MMInventoryEventType.ItemEquipped,
                null, // Slot is not used in this context
                GlobalInventoryManager.DirigibleScannerInventoryName, // Assuming this is the inventory name
                this, // The item being equipped
                1, // Quantity, assuming 1 for equipping
                -1, // Index, not used here
                playerID);

            return base.Equip(playerID);
        }
    }
}
