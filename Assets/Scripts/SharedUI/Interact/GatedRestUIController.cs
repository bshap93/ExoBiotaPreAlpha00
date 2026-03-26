using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using Manager;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.Interact
{
    public class GatedRestUIController : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] public RadialSlider timeLengthSlider;
        [SerializeField] public ButtonManager confirmRestButton;
        [SerializeField] public ButtonManager cancelButton;
        [SerializeField] public ButtonManager restUntilStaminaFullButton;
        [SerializeField] WaitWhileInteractingOverlay waitOverlay;
        [SerializeField] public TMP_Text staminaAmtToBeRestoredText;
        [FormerlySerializedAs("_currentDockId")]
        public string currentDockId;
        [SerializeField] MMFeedbacks restFeedbacks;
        [SerializeField] MMFeedbacks awakenFeedbacks;
        CanvasGroup _canvasGroup;

        int _currentRestTimeMinutes;
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // hide
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.RestTimeSetAmount)
            {
                if (eventType.uiActionType == UIActionType.Open)
                {
                    // show
                    _canvasGroup.alpha = 1;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
                else if (eventType.uiActionType == UIActionType.Close)
                {
                    // hide
                    _canvasGroup.alpha = 0;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }
        public void Initialize(GatedRestDetails restDetails)
        {
            titleText.text = restDetails.interactionName;
            _currentRestTimeMinutes = restDetails.defaultRestTimeMinutes;
            timeLengthSlider.currentValue = _currentRestTimeMinutes;
            timeLengthSlider.UpdateUI();
        }
        public void OnConfirmPressed(GatedRestDetails currentRestDetails)
        {
            MyUIEvent.Trigger(UIType.RestTimeSetAmount, UIActionType.Close);
            MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Open);
            waitOverlay.Show(currentRestDetails.interactionName);
            GatedRestEvent.Trigger(
                GatedInteractionEventType.StartInteraction, currentRestDetails, _currentRestTimeMinutes, currentDockId);

            restFeedbacks?.PlayFeedbacks();

            StartCoroutine(
                waitOverlay.SimulateProgress(
                    currentRestDetails.realWorldWaitDuration, () =>
                    {
                        waitOverlay.Hide();
                        MyUIEvent.Trigger(UIType.RestTimeSetAmount, UIActionType.Close);

                        awakenFeedbacks?.PlayFeedbacks();

                        GatedRestEvent.Trigger(
                            GatedInteractionEventType.CompleteInteraction, currentRestDetails, _currentRestTimeMinutes,
                            currentDockId);
                    }));
        }
        public void OnSetRestUntilStaminaFullPressed(GatedRestDetails currentRestDetails)
        {
            var minsToRest = CalculateMinutesToRestUntilStaminaFull(currentRestDetails);
            _currentRestTimeMinutes = minsToRest;
            timeLengthSlider.currentValue = _currentRestTimeMinutes;
            timeLengthSlider.UpdateUI();
        }
        public void OnTimeLengthSliderChanged(GatedRestDetails currentRestDetails, float value)
        {
            _currentRestTimeMinutes = (int)value;
            var theoreticalStaminaRecovered = _currentRestTimeMinutes * currentRestDetails.staminaRestoredPerMinute;
            var recoverable = GetStaminaAmountRecoverable(theoreticalStaminaRecovered);
            staminaAmtToBeRestoredText.text = $"Stamina to be Restored: {recoverable}";
        }
        static float GetStaminaAmountRecoverable(float theoreticalStaminaRecovered)
        {
            var statManager = PlayerMutableStatsManager.Instance;
            if (statManager != null)
            {
                // Calculate amount of stamina recoverable
                var currentStamina = statManager.CurrentStamina;
                var currentMaxStamina = statManager.BaseMaxStamina;
                var staminaRecoverable = Mathf.Min(theoreticalStaminaRecovered, currentMaxStamina - currentStamina);

                return staminaRecoverable;
            }

            Debug.LogError("No stats manager found for the current player");
            return 0;
        }

        int CalculateMinutesToRestUntilStaminaFull(GatedRestDetails currentRestDetails)
        {
            var statManager = PlayerMutableStatsManager.Instance;
            if (statManager == null)
            {
                Debug.LogError("No stats manager found for the current player");
                return 0;
            }

            var currentStamina = statManager.CurrentStamina;
            var currentMaxStamina = statManager.BaseMaxStamina;

            if (currentStamina >= currentMaxStamina)
            {
                AlertEvent.Trigger(AlertReason.GatedUIActionInvalid, "Stamina is already full.");
                return 0; // No need to rest if stamina is already full
            }

            var staminaNeeded = currentMaxStamina - currentStamina;
            var staminaRegenPerMinute = currentRestDetails.staminaRestoredPerMinute;
            var minsToRest = 0;
            if (staminaRegenPerMinute != 0)
            {
                minsToRest = Mathf.CeilToInt(staminaNeeded / staminaRegenPerMinute);
            }
            else
            {
                Debug.LogError("Stamina regeneration rate is zero.");
                return 0;
            }

            return minsToRest;
        }
    }
}
