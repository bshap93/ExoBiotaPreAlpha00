using Helpers.Events;
using Interfaces;
using Manager;
using Manager.DialogueScene;
using Manager.Global;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.InputsD
{
    public class DefaultInput : MonoBehaviour, IPlayerInput
    {
        public enum InputActions
        {
            Pause,
            IGUIToggle
        }

        const int PauseId = 89;
        const int IGUIToggleId = 90;
        const int UniversalInteractId = 99;

        [FormerlySerializedAs("_isPausePressed")]
        public bool isPausePressed;

        public bool isIGUITogglePressed;

        public bool isUniversalInteractPressed;

        public int playerId;

        public Player DefaultPlayer;

        public static DefaultInput Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;


            DefaultPlayer = ReInput.players.GetPlayer(playerId);
        }

        void Update()
        {
            GetInput();
        }

        void GetInput()
        {
            isPausePressed = DefaultPlayer.GetButtonDown(PauseId);

            isUniversalInteractPressed = DefaultPlayer.GetButton(IGUIToggleId);

            isIGUITogglePressed = DefaultPlayer.GetButtonDown(IGUIToggleId);

            if (DefaultPlayer.GetButtonDown(IGUIToggleId))
                ToggleIGUI();
        }
        public static void ToggleIGUI()
        {
            if (PauseManager.Instance.IsPaused()) return;
            if (DialogueManager.Instance.IsDialogueActive) return;
            if (PlayerUIManager.Instance.modalIsOpen) return;

            if (TutorialManager.Instance != null && TutorialManager.Instance.IsOpen) return;
            // Use PlayerUIManager’s truth
            var iGUIOpen = PlayerUIManager.Instance?.uiIsOpen ?? false;
            var gatedOpen = PlayerUIManager.Instance?.gatedUIIsOpen ?? false;
            var modalOpen = PlayerUIManager.Instance?.modalIsOpen ?? false;


            if (iGUIOpen && !gatedOpen && !modalOpen)
            {
                MyUIEvent.Trigger(
                    UIType.InGameUI, UIActionType.Close
                );
                Time.timeScale =  1;


                UnPauseAudio();
                return;
                

            }

            var canOpen = !gatedOpen && !modalOpen;
            if (canOpen)
            {
                MyUIEvent.Trigger(
                    UIType.InGameUI, UIActionType.Open
                );
                Time.timeScale =  0;


                PauseAudio();
            }
        }

        static void UnPauseAudio()
        {
            AudioEvent.Trigger(AudioEventType.UnPauseAudio);
        }

        static void PauseAudio()
        {
            AudioEvent.Trigger(AudioEventType.PauseAudio);
            // Debug.Log("Audio Paused");
        }
    }
}
