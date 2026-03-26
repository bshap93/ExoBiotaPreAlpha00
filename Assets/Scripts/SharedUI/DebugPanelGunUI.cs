using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events.Combat;
using Manager;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI
{
    public class DebugPanelGunUI : MonoBehaviour, MMEventListener<AmmoEvent>,
        MMEventListener<EnergyGunStateEvent>

    {
        [SerializeField] TMP_Text spareCartridgesText;
        [FormerlySerializedAs("cartridgeTypeText")] [FormerlySerializedAs("currentCartridgeText")] [SerializeField]
        TMP_Text ammoTypeText;
        // [SerializeField] TMP_Text gunModeText;
        [SerializeField] CanvasGroup canvasGroup;


        int _ammoCount;
        AmmoType _currentAmmoType;
        // EnergyGunMode _currentGunMode;

        void Awake()
        {
            Hide();
        }

        void OnEnable()
        {
            this.MMEventStartListening<AmmoEvent>();
            this.MMEventStartListening<EnergyGunStateEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<AmmoEvent>();
            this.MMEventStopListening<EnergyGunStateEvent>();
        }
        public void OnMMEvent(AmmoEvent eventType)
        {
            if (eventType.EventDirectionVar == AmmoEvent.EventDirection.Inbound) return;
            var toolsStateManager = ToolsStateManager.Instance;
            if (toolsStateManager == null) return;
            if (eventType.AmmoType != toolsStateManager.CurrentAmmoType)
            {
                Debug.Log("Ignoring ammo event for different ammo type.");
                return;
            }

            if (eventType.EventType == AmmoEvent.AmmoEventType.InitializedAmmoAmount)
            {
                if (eventType.AmmoType == AmmoType.None)
                {
                    // Debug.Log("Hiding gun UI - no ammo type.");
                    Hide();
                    return;
                }

                _ammoCount = eventType.UnitsOfAmmo;

                _currentAmmoType = eventType.AmmoType;
                spareCartridgesText.text =
                    $"Spare Cartridges: {_ammoCount}";

                ammoTypeText.text =
                    $"Ammo Type: {_currentAmmoType}";
            }
            else if (eventType.EventType == AmmoEvent.AmmoEventType.ConsumedAmmo)
            {
                _ammoCount -= eventType.UnitsOfAmmo;
                spareCartridgesText.text =
                    $"Spare Cartridges: {_ammoCount}";
            }
            else if (eventType.EventType == AmmoEvent.AmmoEventType.PickedUpAmmo)
            {
                _ammoCount += eventType.UnitsOfAmmo;
                spareCartridgesText.text =
                    $"Spare Cartridges: {_ammoCount}";
            }
        }
        public void OnMMEvent(EnergyGunStateEvent eventType)
        {
            if (eventType.EventDirection == AmmoEvent.EventDirection.Inbound) return;
            var toolsStateManager = ToolsStateManager.Instance;
            if (toolsStateManager == null) return;
            if (eventType.EventType == EnergyGunStateEvent.GunStateEventType.ChangedFireMode)
            {
                // _currentGunMode = eventType.NewGunMode;
                // gunModeText.text = $"Gun Mode: {_currentGunMode}";
            }
            else if (eventType.EventType == EnergyGunStateEvent.GunStateEventType.InitializedGunState)
            {
                // _currentGunMode = toolsStateManager.EnergyGunMode;
                // should be magnium energy ammo
                _currentAmmoType = toolsStateManager.CurrentAmmoType;

                // gunModeText.text = $"Gun Mode: {_currentGunMode}";
                ammoTypeText.text =
                    $"Ammo Type: {_currentAmmoType}";

                spareCartridgesText.text =
                    $"Spare Cartridges: {toolsStateManager.MagniumEnergyUnitsAvailable}";

                _ammoCount = toolsStateManager.MagniumEnergyUnitsAvailable;

                Show();
            }
            else if (eventType.EventType == EnergyGunStateEvent.GunStateEventType.UnequippedGun)
            {
                Debug.Log("Hiding gun UI - gun unequipped.");
                Hide();
            }
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
