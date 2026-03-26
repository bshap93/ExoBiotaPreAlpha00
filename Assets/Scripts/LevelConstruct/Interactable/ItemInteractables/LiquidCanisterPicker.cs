using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;

namespace LevelConstruct.Interactable.ItemInteractables
{
    public class LiquidCanisterPicker : ItemPicker.ItemPicker
    {
        public LiquidType liquidTypeContained;
        public float amountOfLiquidCurrentlyInCanisterLiters = 1f;


        public float GetFractionFull()
        {
            if (inventoryItem is LiquidContainerSObject liquidContainer)
                return amountOfLiquidCurrentlyInCanisterLiters / liquidContainer.capacityLiters;

            return 0f;
        }

        // TODO Billboard Shows Liquid Type and Amount (frac)

        // TODO Item in inventory shows Liquid Type and Amount (frac)

        // TODO canister appears empty when liquid is 0
        // TODO canister appears full when liquid is at capacity
    }
}
