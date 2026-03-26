using Helpers.Events.UI;
using MoreMountains.Tools;
using SharedUI.Interact.Map;
using UnityEngine;
using UnityEngine.UI;

namespace NewScript.UI
{
    public class MapPresenterGroup : MonoBehaviour, MMEventListener<MapEvent>
    {
        MapObject _mapObject;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image mapImageBase;
        [SerializeField] RenderTexture mapRenderTexture;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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
        public void OnMMEvent(MapEvent eventType)
        {
            switch (eventType.EventType)
            {
                case MapEvent.MapEventType.OpenedMap:
                    _mapObject = eventType.MapObject;
                    canvasGroup.alpha = 1;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    break;
                case MapEvent.MapEventType.ClosedMap:
                    canvasGroup.alpha = 0;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    break;
            }
        }
    }
}
