using Events;
using Helpers.Events;
using Structs;
using UnityEngine;

namespace SharedUI.Pause
{
    public class PauseOverlayUIController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void TriggerOpenSettings()
        {
            MyUIEvent.Trigger(UIType.GlobalSettingsPanel, UIActionType.Open);
        }

        public void TriggerResumeGame()
        {
        }

        public void TriggerResetAndSave()
        {
            ResetDataEvent.Trigger();
            SaveDataEvent.Trigger();
            AlertEvent.Trigger(AlertReason.SavingGame, "All data wiped successfully!", "Wiped Data");
        }

        public void TriggerFreeLookMode()
        {
            ModeLoadEvent.Trigger(ModeLoadEventType.Enabled, GameMode.FreeLook);
        }
    }
}
