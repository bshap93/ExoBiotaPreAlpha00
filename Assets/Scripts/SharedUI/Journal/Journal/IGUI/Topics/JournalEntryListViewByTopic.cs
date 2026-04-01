using Helpers.Events.PlayerData;
using JournalData.JournalEntries;
using Manager.PlayerDataManagers;
using SharedUI.BaseElement;
using UnityEngine;

namespace SharedUI.Journal.Journal.IGUI.Topics
{
    public class JournalEntryListViewByTopic : SelectionListViewNavigableWithActiveElement<JournalTopicEvent>
    {
        string _topicId;
        public override void OnMMEvent(JournalTopicEvent eventType)
        {
            if (eventType.EventType == JournalTopicEventType.Selected)
            {
                _topicId = eventType.JournalTopicUniqueId;
                Refresh();
            }
        }
        public override void Refresh()
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
