using System;
using System.Collections;
using FirstPersonPlayer;
using Helpers.Events;
using Unity.Cinemachine;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace ModeControllers
{
    public class FirstPersonModeController : ModeController
    {
        public CinemachineCamera vcam;
        RewiredFirstPersonInputs _rewiredInput;

        void OnEnable()
        {
            _rewiredInput = GetComponent<RewiredFirstPersonInputs>();
            if (_rewiredInput == null)
                throw new NullReferenceException(
                    "RewiredFirstPersonInputs component is missing on FirstPersonModeController.");

            SceneEvent.Trigger(SceneEventType.PlayerPawnLoaded);
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
