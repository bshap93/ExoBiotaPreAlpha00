using System.Collections;
using DG.Tweening;
using Helpers.Events.Combat;
using Helpers.Events.Progression;
using Inventory;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI.Progression
{
    public class XphudElement : MonoBehaviour, MMEventListener<XPEvent>,
        MMEventListener<ProgressionUpdateListenerNotifier>, MMEventListener<LevelingEvent>,
        MMEventListener<MMInventoryEvent>, MMEventListener<StaminaRestoreRateEvent>
    {
        [Header("Main Canvas Group")] [SerializeField]
        CanvasGroup debugCanvasGroup;
        [SerializeField] bool debugMode = true;

        [Header("References")] [SerializeField]
        LevelingManager levelingManager;
        [SerializeField] PlayerMutableStatsManager playerMutableStatsManager;

        [SerializeField] XPNotify xpNotifyComponent;
        [SerializeField] LevelNotify levelNotifyComponent;

        [SerializeField] CanvasGroup xpNotifyCanvasGroup;
        [SerializeField] CanvasGroup levelNotifyCanvasGroup;

        [Header("Notification")] [SerializeField]
        GameObject xpNotify;
        [SerializeField] GameObject levelNotify;
        [SerializeField] float fadeInDuration = 0.5f;
        [SerializeField] float fadeOutDuration = 0.5f;


        [Header("Debug")] [SerializeField] CanvasGroup debugChipsCanvasGroup;
        [SerializeField] TMP_Text totalXPText;
        [SerializeField] TMP_Text currentLevelText;
        [SerializeField] TMP_Text staminaRestoreRateText;
        [SerializeField] TMP_Text unusedAttributePointsText;
        [SerializeField] TMP_Text coresNumberText;

        void Start()
        {
            xpNotifyCanvasGroup.alpha = 0;
            levelNotifyCanvasGroup.alpha = 0;
            debugChipsCanvasGroup.alpha = debugMode ? 1 : 0;

            if (levelingManager != null)
            {
                totalXPText.text = levelingManager.CurrentTotalXP.ToString();
                currentLevelText.text = levelingManager.CurrentLevel.ToString();
                // unusedUpgradesText.text = levelingManager.UnspentStatUpgrades.ToString();
                unusedAttributePointsText.text = levelingManager.UnspentAttributePoints.ToString();
                coresNumberText.text = levelingManager.CurentNumberOfCores().ToString();
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<XPEvent>();
            this.MMEventStartListening<ProgressionUpdateListenerNotifier>();
            this.MMEventStartListening<LevelingEvent>();
            this.MMEventStartListening<MMInventoryEvent>();
            this.MMEventStartListening<StaminaRestoreRateEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<XPEvent>();
            this.MMEventStopListening<ProgressionUpdateListenerNotifier>();
            this.MMEventStopListening<LevelingEvent>();
            this.MMEventStopListening<MMInventoryEvent>();
            this.MMEventStopListening<StaminaRestoreRateEvent>();
        }

        public void OnMMEvent(LevelingEvent eventType)
        {
            if (eventType.EventType == LevelingEventType.LevelUp) ShowLevelUpNotification(eventType.NewLevel);
        }
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.TargetInventoryName != GlobalInventoryManager.OuterCoresInventoryName) return;
            if (eventType.InventoryEventType == MMInventoryEventType.ContentChanged)
                coresNumberText.text = levelingManager.CurentNumberOfCores().ToString();
        }
        public void OnMMEvent(ProgressionUpdateListenerNotifier eventType)
        {
            // for debug
            totalXPText.text = eventType.CurrentTotalXP.ToString();
            currentLevelText.text = eventType.CurrentLevel.ToString();
            // unusedUpgradesText.text = eventType.CurrentUpgradesUnused.ToString();
            unusedAttributePointsText.text = eventType.CurrentAttributePointsUnused.ToString();
        }
        public void OnMMEvent(StaminaRestoreRateEvent eventType)
        {
            // For debug: show current stamina restore rate in the debug panel
            if (debugMode) staminaRestoreRateText.text = $"{eventType.CurrentStaminaRestoreRate}";
        }
        public void OnMMEvent(XPEvent eventType)
        {
            if (eventType.EventType == XPEventType.AwardXPToPlayer)
            {
                ShowXPNotification(eventType.Amount);
            }
        }

        void ShowXPNotification(int amount)
        {
            StartCoroutine(ShowXPNotificationCoroutine(amount));
        }

        void ShowLevelUpNotification(int newLevel)
        {
            StartCoroutine(ShowLevelUpNotificationCoroutine(newLevel));
        }

        IEnumerator ShowLevelUpNotificationCoroutine(int newLevel)
        {
            levelNotifyComponent.SetLevelText(newLevel);
            // fades in tween
            levelNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            // notificationCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(2f);
            // fades out
            levelNotifyCanvasGroup.DOFade(0f, fadeOutDuration);
        }

        IEnumerator ShowXPNotificationCoroutine(int amount)
        {
            xpNotifyComponent.SetXPText(amount);
            // fades in tween
            xpNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            // notificationCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(2f);
            // fades out
            xpNotifyCanvasGroup.DOFade(0f, fadeOutDuration);
        }
    }
}
