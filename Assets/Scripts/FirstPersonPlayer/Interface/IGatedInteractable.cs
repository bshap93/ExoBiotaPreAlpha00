using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Manager.UI;

namespace FirstPersonPlayer.Interface
{
    public interface IGatedInteractable
    {
        List<string> HasToolForInteractionInInventory();
        MyBaseItem GetItemByID(string itemID, MoreMountains.InventoryEngine.Inventory inventory);
        bool CanInteract(out GatedInteractionManager.ReasonWhyCannotInteract reason);
    }
}
