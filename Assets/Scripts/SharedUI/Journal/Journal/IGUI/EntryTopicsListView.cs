using Helpers.Events.PlayerData;
using Manager.PlayerDataManagers;
using SharedUI.BaseElement;
using SharedUI.IGUI;
using UnityEngine;

namespace SharedUI.Journal.Journal.IGUI
{
    public class EntryTopicsListView : SelectionListViewNavigableWithActiveElement<JournalTopicEvent>
    {
        public override void OnMMEvent(JournalTopicEvent eventType)
        {
            if (eventType.EventType == JournalTopicEventType.Added ||
                eventType.EventType == JournalTopicEventType.Updated ||
                eventType.EventType == JournalTopicEventType.Initialized)
                Refresh();
        }
        public override void Refresh()
        {
            var journalMgr = JournalEntryManager.Instance;
            if (journalMgr == null) return;

            foreach (Transform child in listTransform) Destroy(child.gameObject);

            foreach (var topic in journalMgr.GetTopicsAquired())
            {
                var go = Instantiate(listViewElementPrefab, listTransform);
                var element = go.GetComponent<EntryTopicPaneListElem>();
                if (element != null)
                    element.Initialize(topic);
            }
        }
    }
}
