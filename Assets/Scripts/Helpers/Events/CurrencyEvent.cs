using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum CurrencyEventType
    {
        AddCurrency,
        RemoveCurrency,
        SetCurrency
    }

    public enum CurrencyType
    {
        Neume,
        Scrap
    }

    public struct CurrencyEvent
    {
        public CurrencyEventType EventType;
        public float Amount;
        public CurrencyType CurrencyType;

        public static void Trigger(CurrencyEventType eventType, float amount,
            CurrencyType currencyType = CurrencyType.Neume)
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
