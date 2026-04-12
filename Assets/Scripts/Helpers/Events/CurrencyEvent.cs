using FirstPersonPlayer.Interactable.ResourceBoxes;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum CurrencyEventType
    {
        AddCurrency,
        RemoveCurrency,
        SetCurrency
    }

    public struct CurrencyEvent
    {
        public CurrencyEventType EventType;
        public float Amount;
        public ResourceCollectionContainerInteractable.ResourceType CurrencyType;

        public static void Trigger(CurrencyEventType eventType, float amount,
            ResourceCollectionContainerInteractable.ResourceType currencyType =
                ResourceCollectionContainerInteractable.ResourceType.Neumat)
        {
            var currencyEvent = new CurrencyEvent
            {
                EventType = eventType,
                Amount = amount,
                CurrencyType = currencyType
            };

            MMEventManager.TriggerEvent(currencyEvent);
        }
    }
}
