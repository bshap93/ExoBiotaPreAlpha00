using Helpers.Events.PlayerData;
using JournalData.JournalTopics;
using Michsky.MUIP;
using SharedUI.BaseElement;
using TMPro;
using UnityEngine;

namespace SharedUI.Journal.Journal.IGUI.Entries
{
    public class EntryTopicPaneListElem : SelectionListElementNavigable<JournalTopic>
    {
        public TMP_Text entryNameText;
        public TMP_Text categoryNameText;
        public ButtonManager selectButton;

        public Color[] categoryTextColors;
        public Color defaultCategoryTextColor;


        public override void Select()
        {
            Debug.Log("You selected " + entryNameText.text);
            JournalTopicEvent.Trigger(JournalTopicEventType.Selected, ObjectData.UniqueID);
        }
        public override void Deselect()
        {
            // TBD
        }
        public override void Initialize(JournalTopic data)
        {
            ObjectData = data;
            entryNameText.text = data.journalTopicName;
            selectButton.onClick.AddListener(Select);
            switch (data.topicType)
            {
                case JournalTopicType.Character:
                    categoryNameText.text = "Character";
                    if (categoryTextColors.Length > 0)
                        categoryNameText.color = categoryTextColors[0];
                    else categoryNameText.color = defaultCategoryTextColor;

                    break;
                case JournalTopicType.Scenario:
                    categoryNameText.text = "Narrative";
                    if (categoryTextColors.Length > 0)
                        categoryNameText.color = categoryTextColors[0];
                    else categoryNameText.color = defaultCategoryTextColor;

                    break;
                case JournalTopicType.Location:
                    categoryNameText.text = "Location";
                    if (categoryTextColors.Length > 0)
                        categoryNameText.color = categoryTextColors[0];
                    else categoryNameText.color = defaultCategoryTextColor;

                    break;
            }
        }
    }
}
