using System.Collections.Generic;
using Helpers.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.Global
{
    public class AlertLogManager : MonoBehaviour, MMEventListener<AlertEvent>
    {
        [SerializeField] MMFeedbacks notEnoughStaminaFeedbacks;
        [SerializeField] MMFeedbacks cannotSampleFurtherFeedbacks;
        readonly List<AlertContent> _alertLog = new();
        public static AlertLogManager Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(AlertEvent eventType)
        {
            switch (eventType.AlertType)
            {
                case AlertType.Basic:
                    _alertLog.Add(eventType.ToAlertContent());
                    HandleAlertBasic(eventType);
                    break;
                case AlertType.ChoiceModal:
                    break;
            }
        }

        void HandleAlertBasic(AlertEvent alertEvent)
        {
            switch (alertEvent.AlertReason)
            {
                case AlertReason.NotEnoughStamina:
                    notEnoughStaminaFeedbacks?.PlayFeedbacks();
                    break;
                case AlertReason.SampleLimitExceeded:
                    cannotSampleFurtherFeedbacks?.PlayFeedbacks();
                    break;
            }
        }
    }
}
