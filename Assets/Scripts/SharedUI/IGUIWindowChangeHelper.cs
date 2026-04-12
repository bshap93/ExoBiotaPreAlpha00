using Helpers.Events.PlayerData;
using Helpers.Events.UpdateUI;
using UnityEngine;

namespace SharedUI
{
    public class IGUIWindowChangeHelper : MonoBehaviour
    {
        public void ChangeWindow(int windowId)
        {
            switch (windowId)
            {
                case 0:
                    UpdateInventoryWindowEvent.Trigger();
                    break;
                case 1:
                    break;
                case 2:
                    JournalTopicEvent.Trigger(JournalTopicEventType.Initialized);
                    break;
                case 3:
                    break;
            }
        }
    }
}
