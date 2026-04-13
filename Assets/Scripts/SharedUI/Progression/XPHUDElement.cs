using System.Collections;
using DG.Tweening;
using Helpers.Events;
using Helpers.Events.Journal;
using Helpers.Events.Progression;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using SharedUI.Trade;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.Progression
{
    public class XphudElement : MonoBehaviour, MMEventListener<XPEvent>,
        MMEventListener<LevelingEvent>,
        MMEventListener<JournalNotificationEvent>, MMEventListener<ResourceCurrencyEvent>
    {
        [SerializeField] PlayerMutableStatsManager playerMutableStatsManager;

        [Header("Component Classes")] [SerializeField]
        XPNotify xpNotifyComponent;
        [SerializeField] LevelNotify levelNotifyComponent;
        [SerializeField] JournalNotify topicNotifyComponent;
        [SerializeField] JournalNotify entryNotifyComponent;
        [FormerlySerializedAs("currencyNotifyComponent")] [SerializeField]
        ResourceNotify resourceNotifyComponent;

        [Header("Canvas Groups")] [SerializeField]
        CanvasGroup xpNotifyCanvasGroup;
        [SerializeField] CanvasGroup levelNotifyCanvasGroup;
        [SerializeField] CanvasGroup topicNotifyCanvasGroup;
        [SerializeField] CanvasGroup entryNotifyCanvasGroup;
        [FormerlySerializedAs("currencyNotifyCanvasGroup")]
        [FormerlySerializedAs("currencyAmountText")]
        [SerializeField]
        CanvasGroup resourceNotifyCanvasGroup;

        [Header("Notification")] [SerializeField]
        GameObject xpNotify;
        [SerializeField] GameObject levelNotify;
        [SerializeField] GameObject topicNotify;
        [SerializeField] GameObject entryNotify;
        [FormerlySerializedAs("currencyNotify")] [SerializeField]
        GameObject resourceNotify;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks showTopicFeedbacks;
        [SerializeField] MMFeedbacks showEntryFeedbacks;


        [Header("Timing")] [SerializeField] float showDuration = 2f;
        [SerializeField] float fadeInDuration = 0.5f;
        [SerializeField] float fadeOutDuration = 0.5f;


        void Start()
        {
            xpNotifyCanvasGroup.alpha = 0;
            levelNotifyCanvasGroup.alpha = 0;
            topicNotifyCanvasGroup.alpha = 0;
            entryNotifyCanvasGroup.alpha = 0;
            resourceNotifyCanvasGroup.alpha = 0;

            xpNotify.SetActive(false);
            levelNotify.SetActive(false);
            topicNotify.SetActive(false);
            entryNotify.SetActive(false);
            resourceNotify.SetActive(false);
        }

        void OnEnable()
        {
            this.MMEventStartListening<XPEvent>();
            this.MMEventStartListening<LevelingEvent>();
            this.MMEventStartListening<JournalNotificationEvent>();
            this.MMEventStartListening<ResourceCurrencyEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<XPEvent>();
            this.MMEventStopListening<LevelingEvent>();
            this.MMEventStopListening<JournalNotificationEvent>();
            this.MMEventStopListening<ResourceCurrencyEvent>();
        }

        public void OnMMEvent(JournalNotificationEvent eventType)
        {
            if (eventType.EntityType == JournalEntityType.Topic)
                ShowNewTopicNotification(eventType.EntityName);
            else if (eventType.EntityType == JournalEntityType.Entry) ShowNewEntryNotification(eventType.EntityName);
        }

        public void OnMMEvent(LevelingEvent eventType)
        {
            if (eventType.EventType == LevelingEventType.LevelUp) ShowLevelUpNotification(eventType.NewLevel);
        }
        public void OnMMEvent(ResourceCurrencyEvent eventType)
        {
            if (eventType.EventType == ResourceCurrencyEventType.AddResource)
                ShowCurrencyNotification(eventType.Amount.ToString("F0"));
        }

        public void OnMMEvent(XPEvent eventType)
        {
            if (eventType.EventType == XPEventType.AwardXPToPlayer) ShowXPNotification(eventType.Amount);
        }

        void ShowXPNotification(int amount)
        {
            StartCoroutine(ShowXPNotificationCoroutine(amount));
        }

        void ShowLevelUpNotification(int newLevel)
        {
            StartCoroutine(ShowLevelUpNotificationCoroutine(newLevel));
        }

        void ShowNewTopicNotification(string topicTxt)
        {
            StartCoroutine(ShowTopicNotificationCoroutine(topicTxt));
        }

        void ShowNewEntryNotification(string entryTxt)
        {
            StartCoroutine(ShowEntryNotificationCoroutine(entryTxt));
        }

        void ShowCurrencyNotification(string currencyTxt)
        {
            StartCoroutine(ShowCurrencyNotificationCoroutine(currencyTxt));
        }

        IEnumerator ShowCurrencyNotificationCoroutine(string currencyTxt)
        {
            resourceNotify.SetActive(true);
            resourceNotifyComponent.SetCurrencyText(currencyTxt);
            // fades in tween
            resourceNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            yield return new WaitForSeconds(showDuration);
            resourceNotifyCanvasGroup.DOFade(0f, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
            resourceNotify.SetActive(false);
        }

        IEnumerator ShowLevelUpNotificationCoroutine(int newLevel)
        {
            levelNotify.SetActive(true);
            levelNotifyComponent.SetLevelText(newLevel);
            // fades in tween
            levelNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            // notificationCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(showDuration);
            // fades out
            levelNotifyCanvasGroup.DOFade(0f, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
            levelNotify.SetActive(false);
        }

        IEnumerator ShowXPNotificationCoroutine(int amount)
        {
            xpNotify.SetActive(true);
            xpNotifyComponent.SetXPText(amount);
            // fades in tween
            xpNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            // notificationCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(showDuration);
            // fades out
            xpNotifyCanvasGroup.DOFade(0f, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
            xpNotify.SetActive(false);
        }

        IEnumerator ShowTopicNotificationCoroutine(string topicTxt)
        {
            topicNotify.SetActive(true);
            showTopicFeedbacks?.PlayFeedbacks();
            topicNotifyComponent.SetJournalEntityText(topicTxt);
            // fades in tween
            topicNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            // notificationCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(showDuration);
            // fades out
            topicNotifyCanvasGroup.DOFade(0f, fadeOutDuration);
            yield return new WaitForSeconds(fadeOutDuration);
            topicNotify.SetActive(false);
        }

        IEnumerator ShowEntryNotificationCoroutine(string entryTxt)
        {
            entryNotify.SetActive(true);
            showEntryFeedbacks?.PlayFeedbacks();
            entryNotifyComponent.SetJournalEntityText(entryTxt);
            // fades in tween
            entryNotifyCanvasGroup.DOFade(1f, fadeInDuration);
            // notificationCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(showDuration);
            // fades out
            entryNotifyCanvasGroup.DOFade(0f, fadeOutDuration);
            yield return new WaitForSeconds(fadeOutDuration);
            entryNotify.SetActive(false);
        }
    }
}
