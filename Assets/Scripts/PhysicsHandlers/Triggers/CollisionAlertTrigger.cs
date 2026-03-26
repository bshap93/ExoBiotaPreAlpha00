using System;
using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;
using Utilities.Interface;

namespace PhysicsHandlers.Triggers
{
    public class CollisionAlertTrigger : MonoBehaviour, MMEventListener<SpontaneousTriggerEvent>, IRequiresUniqueID
    {
        [SerializeField] string uniqueID;
        [SerializeField] string alertMessage;
        [SerializeField] string alertTitle;
        [SerializeField] AlertReason alertReason;
        [SerializeField] AlertType alertType = AlertType.Basic;
        [SerializeField] GameObject alertTriggerCollider;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            alertTriggerCollider.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
            if (eventType.UniqueID == uniqueID) EnableAlertTriggerCollider();
        }
        public void TriggerAlert()
        {
            AlertEvent.Trigger(alertReason, alertMessage, alertTitle, alertType);
        }
        public void EnableAlertTriggerCollider()
        {
            alertTriggerCollider.SetActive(true);
        }
    }
}
