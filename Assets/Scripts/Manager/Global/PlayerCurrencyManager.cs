using FirstPersonPlayer.Interactable.ResourceBoxes;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using SharedUI;
using UnityEngine;

namespace Manager.Global
{
    public class PlayerCurrencyManager : MonoBehaviour, MMEventListener<ResourceCurrencyEvent>, ICoreGameService
    {
        const string PrimaryResourceAmount = "PrimaryResourceAmount";
        const string SecondaryResourceAmount = "SecondaryResourceAmount";

        public CurrencyBarUpdater currencyBarUpdater;

        [SerializeField] bool autoSave;

        public ResourceCollectionContainerInteractable.ResourceType primaryCurrencyType;
        public ResourceCollectionContainerInteractable.ResourceType secondaryCurrencyType;

        public float InitialPrimaryCurrencyAmount;

        public float InitialSecondaryCurrencyAmount;

        bool _dirty;


        string _savePath;
        public float PlayerPrimaryCurrencyAmount { get; private set; }

        public float PlayerSecondaryCurrencyAmount { get; private set; }

        // instance 
        public static PlayerCurrencyManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                //     DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (currencyBarUpdater == null) currencyBarUpdater = FindFirstObjectByType<CurrencyBarUpdater>();
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("[PlayerCurrencyManager] No save file found, forcing initial save...");
                ResetPlayerCurrency(primaryCurrencyType); // Ensure default values are set
                ResetPlayerCurrency(secondaryCurrencyType);
            }

            LoadPlayerCurrency();
        }

        void OnEnable()
        {
            this.MMEventStartListening();

            // Initialize currency bar
            if (currencyBarUpdater != null)
                currencyBarUpdater.Initialize();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }


        public void Save()
        {
            var saveFilePath = GetSaveFilePath();
            ES3.Save(
                PrimaryResourceAmount, PlayerPrimaryCurrencyAmount,
                saveFilePath);

            ES3.Save(
                SecondaryResourceAmount, PlayerSecondaryCurrencyAmount,
                saveFilePath);

            _dirty = false;
        }

        public void Load()
        {
            LoadPlayerCurrency();

            _dirty = false;
        }

        public void Reset()
        {
            ResetData();
        }


        public void MarkDirty()
        {
            _dirty = true;
        }

        string ICoreGameService.GetSaveFilePath()
        {
            return GetSaveFilePath();
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath()) &&
                   ES3.KeyExists(PrimaryResourceAmount, GetSaveFilePath()) &&
                   ES3.KeyExists(SecondaryResourceAmount, GetSaveFilePath());
        }

        public void OnMMEvent(ResourceCurrencyEvent eventType)
        {
            switch (eventType.EventType)
            {
                case ResourceCurrencyEventType.AddResource:
                    AddCurrency(eventType.ResourceType, eventType.Amount);
                    break;
                case ResourceCurrencyEventType.RemoveResource:
                    RemoveCurrency(eventType.ResourceType, eventType.Amount);
                    break;

                case ResourceCurrencyEventType.SetCurrency:
                    SetCurrency(eventType.ResourceType, eventType.Amount);
                    break;
                default:
                    Debug.LogWarning($"Unknown CurrencyEventType: {eventType.EventType}");
                    break;
            }
        }


        public void ResetData()
        {
            PlayerPrimaryCurrencyAmount = InitialPrimaryCurrencyAmount;
            PlayerSecondaryCurrencyAmount = InitialSecondaryCurrencyAmount;


            currencyBarUpdater?.Initialize();

            ConditionalSave();
        }

        public void AddCurrency(ResourceCollectionContainerInteractable.ResourceType resourceType, float amount)
        {
            if (resourceType == primaryCurrencyType)
                PlayerPrimaryCurrencyAmount += amount;

            if (resourceType == secondaryCurrencyType)
                PlayerSecondaryCurrencyAmount += amount;

            _dirty = true;
        }

        public void RemoveCurrency(ResourceCollectionContainerInteractable.ResourceType resourceType, float amount)
        {
            if (resourceType == primaryCurrencyType)
            {
                if (PlayerPrimaryCurrencyAmount - amount < 0)
                {
                    PlayerPrimaryCurrencyAmount = 0;
                    AlertEvent.Trigger(
                        AlertReason.InsufficientFunds,
                        "You don't have enough funds to complete this action.",
                        "Insufficient Funds");
                }
                else
                {
                    PlayerPrimaryCurrencyAmount -= amount;
                }

                _dirty = true;
            }
            else if (resourceType == secondaryCurrencyType)
            {
                if (PlayerSecondaryCurrencyAmount - amount < 0)
                {
                    PlayerSecondaryCurrencyAmount = 0;
                    AlertEvent.Trigger(
                        AlertReason.InsufficientResources,
                        "You don't have enough resources to complete this action.",
                        "Insufficient Resources");
                }
                else
                {
                    PlayerSecondaryCurrencyAmount -= amount;
                }

                _dirty = true;
            }
        }

        public void SetCurrency(ResourceCollectionContainerInteractable.ResourceType resourceType, float amount)
        {
            if (resourceType == primaryCurrencyType)
                PlayerPrimaryCurrencyAmount = amount;

            if (resourceType == secondaryCurrencyType)
                PlayerSecondaryCurrencyAmount = amount;

            _dirty = true;
        }

        static string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.CurrencySave);
        }

        public void LoadPlayerCurrency()
        {
            var saveFilePath = GetSaveFilePath();

            if (ES3.FileExists(saveFilePath) && ES3.KeyExists(PrimaryResourceAmount, saveFilePath))
            {
                PlayerPrimaryCurrencyAmount = ES3.Load<float>(PrimaryResourceAmount, saveFilePath);
                if (currencyBarUpdater != null)
                    currencyBarUpdater.Initialize();
            }
            else
            {
                ResetPlayerCurrency(primaryCurrencyType);
                if (currencyBarUpdater != null)
                    currencyBarUpdater.Initialize();
            }

            if (ES3.FileExists(saveFilePath) && ES3.KeyExists(SecondaryResourceAmount, saveFilePath))
                PlayerSecondaryCurrencyAmount = ES3.Load<float>(SecondaryResourceAmount, saveFilePath);
            else
                ResetPlayerCurrency(secondaryCurrencyType);
        }

        public void ResetPlayerCurrency(ResourceCollectionContainerInteractable.ResourceType resourceType)
        {
            PlayerPrimaryCurrencyAmount = InitialPrimaryCurrencyAmount;
            PlayerSecondaryCurrencyAmount = InitialSecondaryCurrencyAmount;
        }
    }
}
