using System;
using System.Collections;
using Dirigible.Controllers;
using Dirigible.Input;
using Events;
using Helpers.Events;
using MoreMountains.Tools;
using Rewired.Integration.Cinemachine3;
using Structs;
using Unity.Cinemachine;
using UnityEngine;

namespace ModeControllers
{
    public class DirigibleModeController : ModeController, MMEventListener<ModeLoadEvent>, MMEventListener<MyUIEvent>
    {
        public CinemachineCamera vcam;

        [SerializeField] DirigibleInput dirigibleInput;
        [SerializeField] DirigibleMovementController dirigibleMovementController;
        [SerializeField] DirigibleAbilityController dirigibleAbilityController;
        [SerializeField] DirigibleEffectsController dirigibleEffectController;

        [SerializeField] Rigidbody rb;

        [SerializeField] GameObject dockingGear;

        [SerializeField] RewiredCinemachineInputAxisController rewiredCinemachineAxisController;


        void OnEnable()
        {
            this.MMEventStartListening<ModeLoadEvent>();
            this.MMEventStartListening<MyUIEvent>();
            dockingGear.SetActive(false);
        }

        void OnDisable()
        {
            this.MMEventStopListening<ModeLoadEvent>();
            this.MMEventStopListening<MyUIEvent>();
        }

        // Keep in mind, this will not be triggered if this is the 
        // Start of the game's runtime.
        public void OnMMEvent(ModeLoadEvent evt)
        {
            if (evt.EventType != ModeLoadEventType.Enabled) return;

            if (evt.ModeName == GameMode.DirigibleFlight)
            {
                dockingGear.SetActive(false);
                dirigibleMovementController.EnableAltitudeHold(28f);
            }
            else if (evt.ModeName == GameMode.Overview)
            {
                dockingGear.SetActive(true);
                dirigibleMovementController.DisableAltitudeHold();
            }
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.InGameUI)
                switch (eventType.uiActionType)
                {
                    case UIActionType.Open:
                        rewiredCinemachineAxisController.enabled = false;
                        break;
                    case UIActionType.Close:
                        rewiredCinemachineAxisController.enabled = true;
                        break;
                    // case UIActionType.Toggle:
                    //     rewiredCinemachineAxisController.enabled = !rewiredCinemachineAxisController.enabled;
                    //     break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        public override IEnumerator Attach()
        {
            vcam.Priority = 10;
            vcam.gameObject.SetActive(true);

            yield return null;
        }

        public override void Detach()
        {
            vcam.Priority = 0;
            vcam.gameObject.SetActive(false);
        }
    }
}
