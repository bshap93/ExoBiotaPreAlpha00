using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FirstPersonPlayer.UI.Stats
{
    public class PlayerStatsBars : MonoBehaviour, MMEventListener<PlayerStatsSyncEvent>

    {
        [Header("Bars")] [SerializeField] MMProgressBar healthBar;

        [SerializeField] MMProgressBar contaminationBar;
        [SerializeField] MMProgressBar staminaBar;

        [SerializeField] Image contaminationFill;
        [FormerlySerializedAs("lostHealthCapacityBar")] [SerializeField]
        Image lostHealthCapacityFill; // Add this field
        [SerializeField] Image lostStaminaCapacityFill;
        [SerializeField] Image bioAlertIcon;
        [SerializeField] Color maxContaminationBarColor;
        [SerializeField] Color normalContaminationBarColor;
        [SerializeField] MMFeedbacks startDialogueFeedback;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text staminaText;
        [SerializeField] TMP_Text contaminationText;

        [Header("Update")] [Tooltip("Minimum absolute change before we push a UI update")] [SerializeField]
        float epsilon = 0.001f;
        float _lastContamination = float.NaN;

        float _lastHealth = float.NaN;
        float _lastStamina = float.NaN;

        void Start()
        {
            var isMaxContaminated = PlayerMutableStatsManager.Instance.CurrentContamination >=
                                    PlayerMutableStatsManager.Instance.CurrentMaxContamination;

            if (isMaxContaminated)
                bioAlertIcon.enabled = true;
            else
                bioAlertIcon.enabled = false;
        }


        void LateUpdate()
        {
            var stats = PlayerMutableStatsManager.Instance;

            TryUpdateBar(ref _lastHealth, stats.CurrentHealth, 0f, stats.CurrentMaxHealth, healthBar);
            TryUpdateBar(ref _lastStamina, stats.CurrentStamina, 0f, stats.BaseMaxStamina, staminaBar);
            TryUpdateBar(
                ref _lastContamination, stats.CurrentContamination, 0f,
                stats.CurrentMaxContamination, contaminationBar);

            // Update text displays "Current / Max"
            if (healthText != null)
                healthText.text =
                    $"{Mathf.RoundToInt(stats.CurrentHealth)}/{Mathf.RoundToInt(stats.CurrentMaxHealth)}";

            if (staminaText != null)
                staminaText.text =
                    $"{Mathf.RoundToInt(stats.CurrentStamina)}/{Mathf.RoundToInt(stats.BaseMaxStamina)}";

            if (contaminationText != null)
                contaminationText.text =
                    $"{Mathf.RoundToInt(stats.CurrentContamination)}/{Mathf.RoundToInt(stats.CurrentMaxContamination)}";

            // Show lost capacity
            if (lostHealthCapacityFill != null)
            {
                var lostHealthCapacity = Mathf.Max(0, stats.BaseMaxHealth - stats.CurrentMaxHealth);
                var lostCapacityFraction = lostHealthCapacity / stats.BaseMaxHealth;
                var targetWidth = lostCapacityFraction * 118f; // 118 is your max width

                var rectTransform = lostHealthCapacityFill.rectTransform;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            }

            if (lostStaminaCapacityFill != null)
            {
                var lostStaminaCapacity = Mathf.Max(0, stats.BaseMaxStamina - stats.BaseMaxStamina);
                var lostCapacityFraction = lostStaminaCapacity / stats.BaseMaxStamina;
                var targetWidth = lostCapacityFraction * 118f; // 118 is your max width

                var rectTransform = lostStaminaCapacityFill.rectTransform;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            }
        }


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(PlayerStatsSyncEvent e)
        {
            ForceRefreshAll(); // snap bars exactly once after big state changes
        }


        void ForceRefreshAll()
        {
            var stats = PlayerMutableStatsManager.Instance;
            if (stats == null) return;

            // Snap to current (no smoothing) so we start from correct baseline
            if (healthBar != null) healthBar.SetBar(stats.CurrentHealth, 0f, stats.CurrentMaxHealth);
            if (staminaBar != null) staminaBar.SetBar(stats.CurrentStamina, 0f, stats.BaseMaxStamina);


            // Guard against division by zero
            if (contaminationBar != null && stats.CurrentMaxContamination > 0)
                contaminationBar.SetBar(stats.CurrentContamination, 0f, stats.CurrentMaxContamination);

            if (stats.CurrentContamination < stats.CurrentMaxContamination)
                bioAlertIcon.enabled = false;
            else
                bioAlertIcon.enabled = true;


            // Set lost capacity bar
            if (lostHealthCapacityFill != null)
            {
                var lostCapacity = Mathf.Max(0, stats.BaseMaxHealth - stats.CurrentMaxHealth);
                var lostCapacityFraction = lostCapacity / stats.BaseMaxHealth;
                var targetWidth = lostCapacityFraction * 118f;

                var rectTransform = lostHealthCapacityFill.rectTransform;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            }

            if (lostStaminaCapacityFill != null)
            {
                var lostCapacity = Mathf.Max(0, stats.BaseMaxStamina - stats.BaseMaxStamina);
                var lostCapacityFraction = lostCapacity / stats.BaseMaxStamina;
                var targetWidth = lostCapacityFraction * 118f;

                var rectTransform = lostStaminaCapacityFill.rectTransform;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            }

            _lastHealth = stats.CurrentHealth;
            _lastStamina = stats.CurrentStamina;
            _lastContamination = stats.CurrentContamination;
        }

        void TryUpdateBar(ref float last, float current, float min, float max, MMProgressBar bar)
        {
            if (bar == null) return;
            current = Mathf.Clamp(current, min, max);

            // Only push an update when the source value actually changed
            if (float.IsNaN(last) || Mathf.Abs(current - last) > epsilon)
            {
                // Smooth animated update (MMProgressBar handles the tween)
                bar.UpdateBar(current, min, max);
                last = current;
            }
        }
        public void UpdateAllBars()
        {
            ForceRefreshAll();
        }
        public void AlertPlayerToStatRisk(bool isMax, PlayerStatsEvent.PlayerStat statType, string nodeTitle,
            string defaultNPCID)
        {
            if (statType == PlayerStatsEvent.PlayerStat.CurrentContamination)
            {
                if (bioAlertIcon != null) bioAlertIcon.enabled = isMax;
                if (contaminationFill != null)
                    contaminationFill.color = isMax ? maxContaminationBarColor : normalContaminationBarColor;


                AlertEvent.Trigger(
                    alertType: AlertType.PauseAndGiveInfo, alertReason: AlertReason.ContaminationMaxedOut,
                    alertMessage: "You are at maximum contamination levels.", alertTitle: "Max Contamination");
            }
        }

        string GetAppropriateStartNode()
        {
            return "InfectionAlertDialogue";
        }
    }
}
