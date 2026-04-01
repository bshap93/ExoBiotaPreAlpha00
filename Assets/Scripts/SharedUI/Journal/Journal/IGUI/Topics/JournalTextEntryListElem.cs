using JournalData.JournalEntries;
using SharedUI.BaseElement;
using TMPro;

namespace SharedUI.Journal.Journal.IGUI.Topics
{
    public class JournalTextEntryListElem : SelectionListElementNavigable<JournalTextEntry>
    {
        public TMP_Text entryNameText;
        public TMP_Text entryDescriptionText;


        public override void Select()
        {
            // N/A for now
        }
        public override void Deselect()
        {
            // N/A
        }
        public override void Initialize(JournalTextEntry data)
        {
            ObjectData = data;
            entryNameText.text = data.entryTextDescription;
            entryDescriptionText.text = data.entryTextDescription;
            entryNameText.color = data.nameTextColor;
            entryDescriptionText.color = data.descriptionTextColor;
        }
    }
}
