using Events;
using Helpers.Events;
using MoreMountains.Tools;
using Overview.OverviewMode;
using UnityEngine;

namespace Overview.UI
{
    public class OverviewZoomOrBackCgController : MonoBehaviour, MMEventListener<OverviewLocationEvent>,
        MMEventListener<DialogueEvent>
    {
        public enum ControlTarget
        {
            BackButton,
            ZoomedToLocationUI
        }

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] OverviewState[] targetState;
        [SerializeField] ControlTarget controlTarget;

        void OnEnable()
        {
            this.MMEventStartListening<OverviewLocationEvent>();
            this.MMEventStartListening<DialogueEvent>();
            canvasGroup.alpha = 0f; // Initially hide the canvas group
            canvasGroup.interactable = false; // Disable interaction
            canvasGroup.blocksRaycasts = false; // Prevent raycasts
        }

        void OnDisable()
        {
            this.MMEventStopListening<OverviewLocationEvent>();
            this.MMEventStopListening<DialogueEvent>();
        }

        public void OnMMEvent(DialogueEvent e) // NEW
        {
            if (e.EventType == DialogueEventType.DialogueStarted)
            {
                // while talking: back button stays hidden
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else if (e.EventType == DialogueEventType.DialogueFinished)
            {
                // conversation done: reveal Back button so player can Retreat
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void OnMMEvent(OverviewLocationEvent eventType)
        {
            if (eventType.LocationActionType == LocationActionType.Approach)
            {
                canvasGroup.alpha = 1f; // Show the canvas group
                canvasGroup.interactable = true; // Enable interaction
                canvasGroup.blocksRaycasts = true; // Allow raycasts
            }

            // Assuming for now that locations are only one deep
            if (eventType.LocationActionType == LocationActionType.RetreatFrom)
            {
                // Hide the canvas group when retreating from any location
                canvasGroup.alpha = 0f; // Hide the canvas group
                canvasGroup.interactable = false; // Disable interaction
                canvasGroup.blocksRaycasts = false; // Prevent raycasts
            }
        }
    }
}
