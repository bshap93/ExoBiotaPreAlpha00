using Helpers.Events;
using Manager.Global;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI
{
    public class CurrencyBarUpdater : MonoBehaviour, MMEventListener<ResourceCurrencyEvent>
    {
        public bool useTextPlaceholder = true;
        public TMP_Text textPlaceholderCurrency;
        public string currencySymbol = "$"; // Currency symbol, could be $, €, ¥, etc.

        MMProgressBar _bar;
        float _currentCurrency;

        void Awake()
        {
            if (!useTextPlaceholder) _bar = GetComponent<MMProgressBar>();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ResourceCurrencyEvent eventType)
        {
            if (useTextPlaceholder)
                switch (eventType.EventType)
                {
                    case ResourceCurrencyEventType.AddResource:
                        _currentCurrency += eventType.Amount;
                        UpdateCurrencyText();
                        break;
                    case ResourceCurrencyEventType.RemoveResource:
                        _currentCurrency = Mathf.Max(0, _currentCurrency - eventType.Amount);
                        UpdateCurrencyText();
                        break;
                    case ResourceCurrencyEventType.SetCurrency:
                        _currentCurrency = eventType.Amount;
                        UpdateCurrencyText();
                        break;
                }
            else
                switch (eventType.EventType)
                {
                    case ResourceCurrencyEventType.AddResource:
                        _currentCurrency += eventType.Amount;
                        _bar.UpdateBar(_currentCurrency, 0, _currentCurrency * 2); // Dynamic max for visual effect
                        break;
                    case ResourceCurrencyEventType.RemoveResource:
                        _currentCurrency = Mathf.Max(0, _currentCurrency - eventType.Amount);
                        _bar.UpdateBar(_currentCurrency, 0, _currentCurrency * 2); // Dynamic max for visual effect
                        break;
                    case ResourceCurrencyEventType.SetCurrency:
                        _currentCurrency = eventType.Amount;
                        _bar.UpdateBar(_currentCurrency, 0, _currentCurrency * 2); // Dynamic max for visual effect
                        break;
                }
        }

        void UpdateCurrencyText()
        {
            if (textPlaceholderCurrency != null) textPlaceholderCurrency.text = $"{currencySymbol}{_currentCurrency}";
        }

        public void Initialize()
        {
            _currentCurrency = PlayerCurrencyManager.Instance.PlayerPrimaryCurrencyAmount;

            if (useTextPlaceholder)
                UpdateCurrencyText();
            else
                _bar.UpdateBar(_currentCurrency, 0, _currentCurrency * 2); // Dynamic max for visual effect
        }
    }
}
