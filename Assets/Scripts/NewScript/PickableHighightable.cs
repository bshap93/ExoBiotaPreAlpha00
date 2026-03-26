using Helpers.Events;
using LevelConstruct.Highlighting;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using MoreMountains.Tools;
using UnityEngine;

public class PickableHighightable : MonoBehaviour, MMEventListener<HighlightEvent>
{
    public static PickableHighightable Instance { get; private set; }

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
        if (e.HighlightableType == HighlightableType.RhizomicCores)
            SetHighlightState(HighlightableType.RhizomicCores, e.State);
    }
    public void SetHighlightState(HighlightableType highlightableType, bool b)
    {
        // Get all pickables in the scene
        var pickables = FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);
        foreach (var pickable in pickables)
            if (highlightableType == HighlightableType.RhizomicCores)
            {
                var highlight = pickable.GetComponent<HighlightEffectController>();
                if (highlight != null)
                {
                    highlight.SetHighlighted(b);
                    highlight.SetTargetVisible(b);
                }
            }
    }
}
