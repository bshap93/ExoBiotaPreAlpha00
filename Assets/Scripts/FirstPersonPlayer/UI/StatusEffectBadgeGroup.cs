using System.Collections;
using Helpers.Events.Status;
using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.UI
{
    public class StatusEffectBadgeGroup : MonoBehaviour, MMEventListener<StatusDebuffEvent>
    {
        [SerializeField] GameObject poisonBadge;
        [SerializeField] GameObject poisonResistedBadge;
        void Start()
        {
            poisonBadge.SetActive(false);
            poisonResistedBadge.SetActive(false);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(StatusDebuffEvent eventType)
        {
            if (eventType.Debuff == StatusDebuffEvent.DebuffType.Poison)
            {
                if (eventType.Type == StatusDebuffEvent.StatusDebuffEventType.Apply)
                    poisonBadge.SetActive(true);
                else if (eventType.Type == StatusDebuffEvent.StatusDebuffEventType.Remove)
                    poisonBadge.SetActive(false);
                else if (eventType.Type == StatusDebuffEvent.StatusDebuffEventType.Resisted)
                    StartCoroutine(ShowResistedStatusEffect(eventType));
            }
        }

        IEnumerator ShowResistedStatusEffect(StatusDebuffEvent eventType)
        {
            switch (eventType.Debuff)
            {
                case StatusDebuffEvent.DebuffType.Poison:
                    poisonResistedBadge.SetActive(true);
                    yield return new WaitForSeconds(1f);
                    poisonResistedBadge.SetActive(false);
                    break;
            }
        }
    }
}
