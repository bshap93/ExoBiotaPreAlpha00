using Events;
using Helpers.Events;
using Helpers.ScriptableObjects.IconRepositories;
using MoreMountains.Tools;
using OWPData.Structs;
using Structs;
using UnityEngine;

namespace Manager
{
    [DefaultExecutionOrder(0)]
    public class PlayerUIManager : MonoBehaviour, MMEventListener<MyUIEvent>, MMEventListener<ModeLoadEvent>
    {
        public static PlayerUIManager Instance;

        public IconRepository defaultIconRepository;

        public bool uiIsOpen;

        public bool iGUIsOpen;

        public bool modalIsOpen;

        public bool gatedUIIsOpen;

        // Persistent variables


        void Awake()
        {
            if (Instance == null)
                Instance = this;

            else
                Destroy(gameObject);
        }


        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<ModeLoadEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<ModeLoadEvent>();
        }


        public void OnMMEvent(ModeLoadEvent eventType)
        {
            if (eventType.EventType == ModeLoadEventType.Load)
            {
                Debug.Log("Load Mode");
                if (eventType.ModeName == GameMode.DirigibleFlight)
                {
                }

                if (eventType.ModeName == GameMode.FreeLook) gameObject.SetActive(false);
            }
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            switch (eventType.uiActionType)
            {
                case UIActionType.Open:
                    uiIsOpen = true;
                    if (eventType.uiType == UIType.InGameUI && !modalIsOpen)
                        iGUIsOpen = true;
                    else if (eventType.uiType == UIType.ModalBoxChoice)
                        modalIsOpen = true;
                    else if (eventType.uiType == UIType.HarvestableInteractChoice ||
                             eventType.uiType == UIType.BreakableInteractChoice ||
                             eventType.uiType == UIType.MachineInteractChoice ||
                             eventType.uiType == UIType.LevelingUI ||
                             eventType.uiType == UIType.LevelingUIInfected ||
                             eventType.uiType == UIType.WaitWhileInteracting)
                        gatedUIIsOpen = true;

                    break;
                case UIActionType.Close:
                    if (eventType.uiType == UIType.InGameUI)
                        iGUIsOpen = false;
                    else if (eventType.uiType == UIType.ModalBoxChoice)
                        modalIsOpen = false;
                    else if (eventType.uiType == UIType.HarvestableInteractChoice ||
                             eventType.uiType == UIType.BreakableInteractChoice ||
                             eventType.uiType == UIType.MachineInteractChoice ||
                             eventType.uiType == UIType.LevelingUI ||
                             eventType.uiType == UIType.LevelingUIInfected ||
                             eventType.uiType == UIType.WaitWhileInteracting)
                        gatedUIIsOpen = false;

                    if (!iGUIsOpen && !modalIsOpen)
                        uiIsOpen = false;

                    break;
                case UIActionType.Update:
                    // Handle any updates if necessary
                    break;
                // case UIActionType.Toggle:
                //     uiIsOpen = !uiIsOpen;
                //     break;
                default:
                    Debug.LogWarning($"Unhandled UIActionType: {eventType.uiActionType}");
                    break;
            }
        }

        public bool IsAnyUIOpen()
        {
            return uiIsOpen || iGUIsOpen || modalIsOpen || gatedUIIsOpen;
        }
    }
}
