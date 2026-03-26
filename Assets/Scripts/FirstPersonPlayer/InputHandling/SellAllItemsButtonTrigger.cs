using Helpers.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.InputHandling
{
    public class SellAllItemsButtonTrigger : MonoBehaviour
    {
        [FormerlySerializedAs("action")] [SerializeField]
        UnityEvent sellAllAction;

        [FormerlySerializedAs("SellAllFeedbacks")]
        public MMFeedbacks sellAllFeedbacks;

        public MMFeedbacks cannotSellAllFeedbacks;

        MoreMountains.InventoryEngine.Inventory inventory;


        public void TriggerSellAll()
        {
            // if (inventory.)
            // {
            //     CannotSellAll();
            //     return;
            // }
            //
            //
            // sellAllFeedbacks?.PlayFeedbacks();
            // sellAllAction?.Invoke();
            // InventoryEvent.Trigger(InventoryEventType.SellAllItems, inventory.inventoryID);
        }

        public void CannotSellAll()
        {
            AlertEvent.Trigger(
                AlertReason.InventotryEmpty, "You cannot sell all items when the inventory is empty.",
                "Inventory is empty");

            cannotSellAllFeedbacks?.PlayFeedbacks();
        }
    }
}
