using System;
using Helpers.Events.Progression;
using Manager.ProgressionMangers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI.Progression
{
    [Serializable]
    public enum AttributeType
    {
        Strength,
        Agility,
        Dexterity,
        Exobiotic,
        Toughness
    }

    public class AttributePointSetter : MonoBehaviour
    {
        [SerializeField] AttributeType attributeType;
        [SerializeField] TMP_Text attributePointText;
        [FormerlySerializedAs("xpNeededForNextIncrease")] [FormerlySerializedAs("attributXPText")] [SerializeField]
        TMP_Text xpNeededForNextIncreaseText;
        [SerializeField] Button increaseButton;
        [SerializeField] Button decreaseButton;
        [SerializeField] CanvasGroup infoBoxCanvasGroup;

        public bool canDecrease;

        int _currentPoints;
        int _currentXP;
        int _xpNeededForNextIncrease;

        public int PendingChanges { get; private set; }
        public AttributeType AttributeType => attributeType;

        void Start()
        {
            HideInfoBox();
        }
        public void HideInfoBox()
        {
            infoBoxCanvasGroup.alpha = 0f;
            infoBoxCanvasGroup.blocksRaycasts = false;
            infoBoxCanvasGroup.interactable = false;
        }

        public void ShowInfoBox()
        {
            infoBoxCanvasGroup.alpha = 1f;
            infoBoxCanvasGroup.blocksRaycasts = true;
            infoBoxCanvasGroup.interactable = true;
        }

        public void Initialize(int currentPoints, int currentUnusedXP)
        {
            var attributeManager = AttributesManager.Instance;
            _currentPoints = currentPoints;
            PendingChanges = 0;


            _xpNeededForNextIncrease = attributeManager.GetXpRequiredForLevel(_currentPoints + 1);
            xpNeededForNextIncreaseText.text =
                _xpNeededForNextIncrease.ToString();

            UpdateDisplay();

            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
            increaseButton.onClick.AddListener(() => OnIncreaseButtonClicked());
            decreaseButton.onClick.AddListener(() => OnDecreaseButtonClicked());

            var canIncrease = _xpNeededForNextIncrease <= currentUnusedXP;
            UpdateButtonStates(canDecrease, canIncrease);
        }

        public void UpdateButtonStates(bool decreaseEnable, bool increaseEnable)
        {
            increaseButton.interactable = true;
            decreaseButton.interactable = true;
        }

        void UpdateDisplay()
        {
            attributePointText.text = _currentPoints.ToString();
            // TODO: Calculate XP needed for next increase
            // xpNeededForNextIncrease.text = _currentXP.ToString();
        }

        void OnIncreaseButtonClicked()
        {
            AttrPendingBuyEvent.Trigger(
                attributeType, PendingBuyEventType.IncreasePendingAttribute, _currentPoints + 1);
        }

        void OnDecreaseButtonClicked()
        {
            // Logic to decrease attribute points
            AttrPendingBuyEvent.Trigger(
                attributeType, PendingBuyEventType.DecreasePendingAttribute,
                _currentPoints - 1);
        }
    }
}
