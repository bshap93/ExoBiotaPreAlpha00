using Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Utilities.CameraUtils
{
    public class CanvasCameraProvider : MonoBehaviour, MMEventListener<ModeLoadEvent>
    {
        private Canvas canvas;
        [SerializeField] private float planeDistance = 1f;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("Canvas component not found on CanvasCameraProvider.");
            }
            else
            {
                // Set the render mode to Screen Space - Camera
                canvas.worldCamera = Camera.main; 
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.sortingLayerName = "UILayer";
                canvas.planeDistance = planeDistance;
            }
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ModeLoadEvent eventType)
        {
        }
    }
}