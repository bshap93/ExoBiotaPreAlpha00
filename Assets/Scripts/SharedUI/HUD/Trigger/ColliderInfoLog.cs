using System;
using Helpers.Events;
using Helpers.ScriptableObjects;
using UnityEngine;
using Utilities.Interface;

namespace SharedUI.HUD.Trigger
{
    public class ColliderInfoLog : MonoBehaviour, IRequiresUniqueID
    {
        [Header("Unique ID")] public string uniqueID;
        public InfoLogContent infoLogContent;

        bool _triggered;


        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer"))
            {
                if (_triggered) return;
                InfoLogEvent.Trigger(infoLogContent, InfoLogEventType.SetInfoLogContent);
                MyUIEvent.Trigger(UIType.InfoLogTablet, UIActionType.Open);
                _triggered = true;
            }
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
    }
}
