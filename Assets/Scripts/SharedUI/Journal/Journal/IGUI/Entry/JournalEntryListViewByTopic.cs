using Helpers.Events;
using Helpers.Events.PlayerData;
using JournalData.JournalEntries;
using Manager.PlayerDataManagers;
using MoreMountains.Tools;
using SharedUI.BaseElement;
using SharedUI.Journal.Journal.IGUI.Topics;
using UnityEngine;

namespace SharedUI.Journal.Journal.IGUI.Entry
{
    public class JournalEntryListViewByTopic : SelectionListViewNavigableWithActiveElement<JournalTopicEvent>,
        MMEventListener<LoadedManagerEvent>
    {
        string _topicId;

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
            if (eventType.EventType == JournalTopicEventType.Selected)
            {
                _topicId = eventType.JournalTopicUniqueId;
                Refresh();
            }
        }
        protected override void Refresh()
        {
            var journalMgr = JournalEntryManager.Instance;
            if (journalMgr == null) return;

            foreach (Transform child in listTransform) Destroy(child.gameObject);

            foreach (var journalEntry in journalMgr.GetEntriesAquired(_topicId))
            {
                var go = Instantiate(listViewElementPrefab, listTransform);
                var element = go.GetComponent<JournalTextEntryListElem>();
                if (element != null)
                    element.Initialize(journalEntry as JournalTextEntry);
            }
        }
    }
}
