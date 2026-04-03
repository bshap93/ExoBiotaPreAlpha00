using Helpers.Events;
using Helpers.Events.PlayerData;
using Manager.PlayerDataManagers;
using MoreMountains.Tools;
using SharedUI.BaseElement;
using SharedUI.Journal.Journal.IGUI.Entries;
using UnityEngine;

namespace SharedUI.Journal.Journal.IGUI.Topic
{
    public class EntryTopicsListView : SelectionListViewNavigableWithActiveElement<JournalTopicEvent>,
        MMEventListener<LoadedManagerEvent>
    {
        public override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<LoadedManagerEvent>();
        }
        public override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<LoadedManagerEvent>();
        }
        public override void OnMMEvent(JournalTopicEvent eventType)
        {
            if (eventType.EventType == JournalTopicEventType.Added ||
                eventType.EventType == JournalTopicEventType.Updated ||
                eventType.EventType == JournalTopicEventType.Initialized)
                Refresh();
        }
        protected override void Refresh()
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
