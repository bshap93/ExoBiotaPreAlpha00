using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.Combat;
using Manager.Status;
using Manager.Status.Scriptable;
using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.UI.PlayerStatus
{
    public class StatusEffectIGUIList : MonoBehaviour, MMEventListener<PlayerStatusEffectEvent>,
        MMEventListener<LoadedManagerEvent>
    {
        public Transform listTransform;

        public GameObject statusEffectElementPrefab;

        public List<StatusEffectIGUIElement> currentElements = new();

        void OnEnable()
        {
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<PlayerStatusEffectEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<PlayerStatusEffectEvent>();
            // Cleanup();
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            var allCurrentStatusEffects = PlayerStatusEffectManager.Instance.GetAllCurrentStatusEffects();
            if (eventType.ManagerType == ManagerType.All)
                Populate(allCurrentStatusEffects);
        }
        public void OnMMEvent(PlayerStatusEffectEvent eventType)
        {
            if (eventType.Direction == PlayerStatusEffectEvent.DirectionOfEvent.Inbound) return;
            if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.Apply)
                Populate(PlayerStatusEffectManager.Instance.GetAllCurrentStatusEffects());
            else if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.Remove)
                Populate(PlayerStatusEffectManager.Instance.GetAllCurrentStatusEffects());
        }

        public void PopulateAllCurrent()
        {
            if (PlayerStatusEffectManager.Instance == null) return;
            Populate(PlayerStatusEffectManager.Instance.GetAllCurrentStatusEffects());
        }
        public void Populate(StatusEffect[] statusEffects)
        {
            Cleanup();
            if (statusEffects == null || statusEffects.Length == 0) return;

            foreach (var effect in statusEffects)
            {
                var newElement = Instantiate(statusEffectElementPrefab, listTransform);
                var elementComponent = newElement.GetComponent<StatusEffectIGUIElement>();
                currentElements.Add(elementComponent);
                if (elementComponent != null) elementComponent.Populate(effect);
            }
        }

        void Cleanup()
        {
            foreach (Transform child in listTransform) Destroy(child.gameObject);
            currentElements.Clear();
        }
    }
}
