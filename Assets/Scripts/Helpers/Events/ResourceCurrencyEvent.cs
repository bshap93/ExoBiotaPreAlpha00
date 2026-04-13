using FirstPersonPlayer.Interactable.ResourceBoxes;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum ResourceCurrencyEventType
    {
        AddResource,
        RemoveResource,
        SetCurrency
    }

    public struct ResourceCurrencyEvent
    {
        public ResourceCurrencyEventType EventType;
        public float Amount;
        public ResourceCollectionContainerInteractable.ResourceType CurrencyType;

        public static void Trigger(ResourceCurrencyEventType eventType, float amount,
            ResourceCollectionContainerInteractable.ResourceType currencyType =
                ResourceCollectionContainerInteractable.ResourceType.Neumat)
        {
            var currencyEvent = new ResourceCurrencyEvent
            {
                EventType = eventType,
                Amount = amount,
                CurrencyType = currencyType
            };

            MMEventManager.TriggerEvent(currencyEvent);
        }
    }
}
