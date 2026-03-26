using Helpers.Events;
using MoreMountains.Tools;
using Rewired.Integration.Cinemachine3;
using UnityEngine;

namespace NewScript.Deprecated
{
    public class DirigibleCameraEventHandler : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        RewiredCinemachineInputAxisController _rewiredCinemachineInputAxisController;
        int _uiElementsOpen;
        void Awake()
        {
            _rewiredCinemachineInputAxisController = GetComponent<RewiredCinemachineInputAxisController>();
            _uiElementsOpen = 0;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiActionType == UIActionType.Open)
            {
                _uiElementsOpen++;
                if (_uiElementsOpen > 0)
                    _rewiredCinemachineInputAxisController.enabled = false;
            }
            else if (eventType.uiActionType == UIActionType.Close)
            {
                _uiElementsOpen--;
                if (_uiElementsOpen <= 0)
                    _rewiredCinemachineInputAxisController.enabled = true;
            }
        }
    }
}
