using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.Inventory;
using Helpers.Events.Spawn;
using Helpers.Events.Status;
using Helpers.Events.UI;
using Helpers.Interfaces;
using Inventory;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    /// <summary>
    ///     Manages the state that cannot be expressed by inventory items.
    /// </summary>
    public class ToolsStateManager : MonoBehaviour, ICoreGameService, MMEventListener<AmmoEvent>,
        MMEventListener<EnergyGunStateEvent>, MMEventListener<BioSampleEvent>, MMEventListener<SecondaryActionEvent>
    {
        [SerializeField] bool autoSave; // checkpoint-only by default

        [SerializeField] GlobalInventoryManager globalInventoryManager;
        [SerializeField] MoreMountains.InventoryEngine.Inventory ammoInventory;

        [SerializeField] int startingMagniumEnergyUnits = 20;


        [SerializeField] int initialMaximumIchorCharges = 4;

        [SerializeField] float initialAmtContaminationPerNotch = 4;


        readonly AmmoSupply _magniumEnergyAmmoSupply = new();
        readonly EnergyGunMode _startingEnergyGunMode =
            EnergyGunMode.Stun;

        int _currentIchorCharges;

        bool _dirty;
        int _maxIchorCharges;


        string _savePath;

        public AmmoType CurrentAmmoType { get; set; }

        public int MagniumEnergyUnitsAvailable
        {
            get => _magniumEnergyAmmoSupply.UnitsAvailable;
            private set => _magniumEnergyAmmoSupply.UnitsAvailable = value;
        }

        public int CurrentIchorCharges
        {
            get => _currentIchorCharges;
            set
            {
                _currentIchorCharges = value;
                MarkDirty();
            }
        }

        public int MaxIchorCharges
        {
            get => _maxIchorCharges;
            set
            {
                _maxIchorCharges = value;
                MarkDirty();
            }
        }

        public EnergyGunMode EnergyGunMode { get; set; }
        public static ToolsStateManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[ToolStateManager] No save file found, forcing initial save...");
                Reset(); // Ensure default values are set
            }

            Load();
        }
        void OnEnable()
        {
            this.MMEventStartListening<AmmoEvent>();
            this.MMEventStartListening<EnergyGunStateEvent>();
            this.MMEventStartListening<BioSampleEvent>();
            this.MMEventStartListening<SecondaryActionEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<AmmoEvent>();
            this.MMEventStopListening<EnergyGunStateEvent>();
            this.MMEventStopListening<BioSampleEvent>();
            this.MMEventStopListening<SecondaryActionEvent>();
        }

        public void Save()
        {
            _savePath = GetSaveFilePath();
            ES3.Save("CurrentAmmoType", (int)CurrentAmmoType, _savePath);
            ES3.Save("MagniumAmmoUnitsAvailable", MagniumEnergyUnitsAvailable, _savePath);
            ES3.Save("EnergyPistolMode", (int)EnergyGunMode, _savePath);
            ES3.Save("CurrentIchorCharges", CurrentIchorCharges, _savePath);
            ES3.Save("MaxIchorCharges", MaxIchorCharges, _savePath);
            _dirty = false;
        }
        public void Load()
        {
            _savePath = GetSaveFilePath();
            if (ES3.KeyExists("CurrentAmmoType", _savePath))
                CurrentAmmoType = (AmmoType)ES3.Load<int>("CurrentAmmoType", _savePath);
            else
                CurrentAmmoType = AmmoType.MagniumEnergyAmmoUnits;

            if (ES3.KeyExists("MagniumAmmoUnitsAvailable", _savePath))
                MagniumEnergyUnitsAvailable = ES3.Load<int>("MagniumAmmoUnitsAvailable", _savePath);
            else
                MagniumEnergyUnitsAvailable = startingMagniumEnergyUnits;

            if (ES3.KeyExists("EnergyPistolMode", _savePath))
                EnergyGunMode = (EnergyGunMode)ES3.Load<int>("EnergyPistolMode", _savePath);
            else
                EnergyGunMode = _startingEnergyGunMode;

            if (CurrentAmmoType == AmmoType.MagniumEnergyAmmoUnits)

                AmmoEvent.Trigger(
                    AmmoEvent.EventDirection.Outbound, MagniumEnergyUnitsAvailable,
                    AmmoEvent.AmmoEventType.InitializedAmmoAmount, AmmoType.MagniumEnergyAmmoUnits);
            else if (CurrentAmmoType == AmmoType.None)
                AmmoEvent.Trigger(
                    AmmoEvent.EventDirection.Outbound, -1,
                    AmmoEvent.AmmoEventType.InitializedAmmoAmount, AmmoType.None);


            if (ES3.KeyExists("CurrentIchorCharges", _savePath))
                CurrentIchorCharges = ES3.Load<int>("CurrentIchorCharges", _savePath);
            else
                CurrentIchorCharges = 0;


            if (ES3.KeyExists("MaxIchorCharges", _savePath))
                MaxIchorCharges = ES3.Load<int>("MaxIchorCharges", _savePath);
            else
                MaxIchorCharges = initialMaximumIchorCharges;


            _dirty = false;
        }


        public void Reset()
        {
            MagniumEnergyUnitsAvailable = startingMagniumEnergyUnits;
            EnergyGunMode = _startingEnergyGunMode;
            CurrentIchorCharges = 0;
            MaxIchorCharges = initialMaximumIchorCharges;
            MarkDirty();
            ConditionalSave();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ToolsStateSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty)
            {
                Save();
                _dirty = false;
            }
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
            ;
        }
        public void OnMMEvent(AmmoEvent eventType)
        {
            if (eventType.EventDirectionVar == AmmoEvent.EventDirection.Outbound) return;

            if (eventType.EventType == AmmoEvent.AmmoEventType.ConsumedAmmo)
                switch (eventType.AmmoType)
                {
                    case AmmoType.MagniumEnergyAmmoUnits:
                        MagniumEnergyUnitsAvailable -= eventType.UnitsOfAmmo;
                        AmmoEvent.Trigger(
                            AmmoEvent.EventDirection.Outbound, eventType.UnitsOfAmmo,
                            AmmoEvent.AmmoEventType.ConsumedAmmo, eventType.AmmoType);

                        MarkDirty();
                        break;
                }
            else if (eventType.EventType == AmmoEvent.AmmoEventType.PickedUpAmmo)
                switch (eventType.AmmoType)
                {
                    case AmmoType.MagniumEnergyAmmoUnits:
                        MagniumEnergyUnitsAvailable += eventType.UnitsOfAmmo;
                        AmmoEvent.Trigger(
                            AmmoEvent.EventDirection.Outbound, eventType.UnitsOfAmmo,
                            AmmoEvent.AmmoEventType.PickedUpAmmo, eventType.AmmoType);

                        MarkDirty();
                        break;
                }
        }

        public void OnMMEvent(BioSampleEvent eventType)
        {
            if (eventType.EventType == BioSampleEventType.CompleteCollection)
                if (_currentIchorCharges < _maxIchorCharges)
                {
                    CurrentIchorCharges++;
                    LiquidToolStateEvent.Trigger(
                        LiquidToolStateEventType.UpdatedIchorCharges, CurrentIchorCharges, MaxIchorCharges);

                    MarkDirty();
                }
        }

        public void OnMMEvent(EnergyGunStateEvent eventType)
        {
            if (eventType.EventDirection == AmmoEvent.EventDirection.Outbound) return;
            if (eventType.EventType == EnergyGunStateEvent.GunStateEventType.UnequippedGun)
            {
                CurrentAmmoType = AmmoType.None;
                // EnergyGunMode = EnergyGunMode.None;
                MarkDirty();
                EnergyGunStateEvent.Trigger(
                    AmmoEvent.EventDirection.Outbound, EnergyGunMode.None,
                    EnergyGunStateEvent.GunStateEventType.UnequippedGun,
                    AmmoType.None
                );
            }
            else if (eventType.EventType == EnergyGunStateEvent.GunStateEventType.InitializedGunState)
            {
                EnergyGunMode = eventType.NewGunMode;
                CurrentAmmoType = eventType.AmmoType;
                MarkDirty();
                EnergyGunStateEvent.Trigger(
                    AmmoEvent.EventDirection.Outbound, EnergyGunMode,
                    EnergyGunStateEvent.GunStateEventType.InitializedGunState,
                    CurrentAmmoType);
            }
            else if (eventType.EventType == EnergyGunStateEvent.GunStateEventType.EquippedGun)
            {
                EnergyGunMode = eventType.NewGunMode;
                CurrentAmmoType = eventType.AmmoType;
                MarkDirty();
                EnergyGunStateEvent.Trigger(
                    AmmoEvent.EventDirection.Outbound, EnergyGunMode,
                    EnergyGunStateEvent.GunStateEventType.EquippedGun,
                    CurrentAmmoType);
            }
        }

        public void OnMMEvent(SecondaryActionEvent eventType)
        {
            if (eventType.SecondaryActionType == SecondaryActionType.InjectAvailableIchor)
            {
                var contaminationToRestrore = CurrentIchorCharges * initialAmtContaminationPerNotch;

                CurrentIchorCharges = 0;
                LiquidToolStateEvent.Trigger(
                    LiquidToolStateEventType.UpdatedIchorCharges, CurrentIchorCharges, MaxIchorCharges);

                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination, PlayerStatsEvent.PlayerStatChangeType.Increase,
                    contaminationToRestrore);

                ContaminationSpikeEvent.Trigger(contaminationToRestrore);


                MarkDirty();
            }
        }


        public class AmmoSupply
        {
            public AmmoType AmmoSupplyType;
            public int UnitsAvailable;
        }
    }
}
