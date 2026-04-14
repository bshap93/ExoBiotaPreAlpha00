using Helpers.Events;
using Helpers.Events.Gated;
using Manager.ProgressionMangers;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScript.UI
{
    public class AttrIGUIList : MonoBehaviour, MMEventListener<GatedLevelingEvent>, MMEventListener<LoadedManagerEvent>
    {
        [SerializeField] TMP_Text strengthText;
        [SerializeField] TMP_Text agilityText;
        [SerializeField] TMP_Text dexterityText;
        [FormerlySerializedAs("mentalToughnessText")] [SerializeField]
        TMP_Text toughnessText;
        [SerializeField] TMP_Text exobioticText;
        [SerializeField] TMP_Text willpowerText;


        void OnEnable()
        {
            this.MMEventStartListening<GatedLevelingEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<GatedLevelingEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
        }
        public void OnMMEvent(GatedLevelingEvent eventType)
        {
            if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                strengthText.text = eventType.AttributeValues.strength.ToString();
                agilityText.text = eventType.AttributeValues.agility.ToString();
                dexterityText.text = eventType.AttributeValues.dexterity.ToString();
                toughnessText.text = eventType.AttributeValues.toughness.ToString();
                exobioticText.text = eventType.AttributeValues.exobiotic.ToString();
                willpowerText.text = eventType.AttributeValues.willpower.ToString();
            }
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All) Initialize();
        }

        public void Initialize()
        {
            var attrMgr = AttributesManager.Instance;
            strengthText.text = attrMgr.Strength.ToString();
            agilityText.text = attrMgr.Agility.ToString();
            dexterityText.text = attrMgr.Dexterity.ToString();
            toughnessText.text = attrMgr.Toughness.ToString();
            exobioticText.text = attrMgr.Exobiotic.ToString();
            willpowerText.text = attrMgr.Willpower.ToString();
        }
    }
}
