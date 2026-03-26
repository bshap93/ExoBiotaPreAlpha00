using Helpers.Events.Status;
using MoreMountains.Tools;
using UnityEngine;

public class StatusEffectBadgeGroup : MonoBehaviour, MMEventListener<StatusDebuffEvent>
{
    [SerializeField] GameObject poisonBadge;
    void Start()
    {
        poisonBadge.SetActive(false);
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
        }
    }
}
