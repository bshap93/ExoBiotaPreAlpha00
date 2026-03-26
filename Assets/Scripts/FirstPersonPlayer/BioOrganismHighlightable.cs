using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;

public class BioOrganismHighlightable : MonoBehaviour, MMEventListener<HighlightEvent>
{
    public static BioOrganismHighlightable Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    void OnEnable()
    {
        this.MMEventStartListening();
    }
    void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(HighlightEvent e)
    {
        if (e.HighlightableType != HighlightableType.RhizomicCores)
            return;

        SetHighlightState(this, e.State);
    }

    void SetHighlightState(object rhizomicCores, bool b)
    {
    }
}
