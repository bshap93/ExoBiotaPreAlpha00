using FirstPersonPlayer.Interactable.ResourceBoxes;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using ScriptableObjects;
using SharedUI;
using UnityEngine;

namespace Manager.Global
{
    public class PlayerCurrencyManager : MonoBehaviour, MMEventListener<ResourceCurrencyEvent>, ICoreGameService
    {
        const string KeyDollarAmount = "PlayerDollarAmount";

        public CurrencyBarUpdater currencyBarUpdater;

        [SerializeField] bool autoSave;

        public ResourceCollectionContainerInteractable.ResourceType primaryCurrencyType;
        public ResourceCollectionContainerInteractable.ResourceType secondaryCurrencyType;

        CharacterStatProfile _characterStatProfile;
        bool _dirty;


        string _savePath;
        public float PlayerPrimaryCurrencyAmount { get; private set; }

        public float InitialPrimaryCurrencyAmount { get; private set; }

        public float PlayerSecondaryCurrencyAmount { get; private set; }

        public float InitialSecondaryCurrencyAmount { get; private set; }

        // instance 
        public static PlayerCurrencyManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
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
                ResetPlayerCurrency(); // Ensure default values are set
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
                KeyDollarAmount, PlayerPrimaryCurrencyAmount,
                saveFilePath);
        }

        public void Load()
        {
            LoadPlayerCurrency();
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
                   ES3.KeyExists(KeyDollarAmount, GetSaveFilePath());
        }

        public void OnMMEvent(ResourceCurrencyEvent eventType)
        {
            switch (eventType.EventType)
            {
                case ResourceCurrencyEventType.AddResource:
                    AddCurrency(eventType.Amount);
                    break;
                case ResourceCurrencyEventType.RemoveResource:
                    RemoveCurrency(eventType.Amount);
                    break;

                case ResourceCurrencyEventType.SetCurrency:
                    SetCurrency(eventType.Amount);
                    break;
                default:
                    Debug.LogWarning($"Unknown CurrencyEventType: {eventType.EventType}");
                    break;
            }
        }


        public void ResetData()
        {
            _characterStatProfile =
                SaveManager.Instance.initialCharacterStatProfile;

            if (_characterStatProfile != null)
                InitialPrimaryCurrencyAmount = _characterStatProfile.InitialCurrency;
            else
                Debug.LogError("CharacterStatProfile not set in PlayerCurrencyManager");


            currencyBarUpdater?.Initialize();

            ConditionalSave();
        }

        public void Initialize()
        {
            ResetPlayerCurrency();
            currencyBarUpdater?.Initialize();
        }

        public void AddCurrency(float amount)
        {
            PlayerPrimaryCurrencyAmount += amount;
            // Add an event trigger to notify UI and other systems

            _dirty = true;
        }

        public void LoseCurrency(float amount)
        {
            if (PlayerPrimaryCurrencyAmount - amount < 0)
                PlayerPrimaryCurrencyAmount = 0;
            else
                PlayerPrimaryCurrencyAmount -= amount;

            _dirty = true;
        }

        public void RemoveCurrency(float amount)
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
        }

        public void SetCurrency(float amount)
        {
            PlayerPrimaryCurrencyAmount = amount;
        }

        static string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.CurrencySave);
        }

        public void LoadPlayerCurrency()
        {
            var saveFilePath = GetSaveFilePath();

            if (ES3.FileExists(saveFilePath) && ES3.KeyExists(KeyDollarAmount, saveFilePath))
            {
                PlayerPrimaryCurrencyAmount = ES3.Load<float>(KeyDollarAmount, saveFilePath);
                if (currencyBarUpdater != null)
                    currencyBarUpdater.Initialize();
            }
            else
            {
                ResetPlayerCurrency();
                if (currencyBarUpdater != null)
                    currencyBarUpdater.Initialize();
            }
        }

        public void ResetPlayerCurrency()
        {
            var characterStatProfile =
                SaveManager.Instance.initialCharacterStatProfile;

            if (characterStatProfile == null)
            {
                Debug.LogError("CharacterStatProfile not found! Using default values.");
                PlayerPrimaryCurrencyAmount = 0; // Default fallback
            }
            else
            {
                PlayerPrimaryCurrencyAmount = characterStatProfile.InitialCurrency;
            }
        }
    }
}
