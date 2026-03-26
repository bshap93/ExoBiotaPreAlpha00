using Helpers.Events.Combat;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    public enum AmmoType
    {
        None,
        MagniumEnergyAmmoUnits
    }

    [CreateAssetMenu(
        fileName = "AmmoItem",
        menuName = "Scriptable Objects/Items/AmmoItem",
        order = 0)]
    public class AmmoItem : MyBaseItem
    {
        public int unitsOfAmmoPerItem = 8;

        public AmmoType ammoType;

        public int UnitsOfAmmo => unitsOfAmmoPerItem;

        public override bool Pick(string playerID)
        {
            AmmoEvent.Trigger(
                AmmoEvent.EventDirection.Inbound, UnitsOfAmmo, AmmoEvent.AmmoEventType.PickedUpAmmo, ammoType);

            // what happens?
            return true;
        }
    }
}
