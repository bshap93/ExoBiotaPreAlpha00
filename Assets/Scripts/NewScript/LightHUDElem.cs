using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class LightHUDElem : MonoBehaviour, MMEventListener<LightEvent>
{
    [SerializeField] Sprite onLightIcon;
    [SerializeField] Sprite offLightIcon;
    [SerializeField] Image lightIconImage;
    void Start()
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

    public void OnMMEvent(LightEvent lightEvent)
    {
        switch (lightEvent.EventType)
        {
            case LightEventType.TurnOn:
                if (lightIconImage != null && onLightIcon != null)
                    lightIconImage.sprite = onLightIcon;

                break;
            case LightEventType.TurnOff:
                if (lightIconImage != null && offLightIcon != null)
                    lightIconImage.sprite = offLightIcon;

                break;
        }
    }
}
