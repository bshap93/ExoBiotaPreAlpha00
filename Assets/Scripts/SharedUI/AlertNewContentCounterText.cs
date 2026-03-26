using Helpers.Events;
using Manager.SceneManagers.Dock;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI
{
    public class AlertNewContentCounterText : MonoBehaviour //, MMEventListener<DockingEvent>
    {
        [SerializeField] TMP_Text text;
        string _dockId;
        string _locationId;


        void OnEnable()
        {
            // this.MMEventStartListening();
        }

        void OnDisable()
        {
            // this.MMEventStopListening();
        }

        // public void OnMMEvent(DockingEvent dockingEvent)
        // {
        //     if (dockingEvent.EventType == DockingEventType.FinishedDocking)
        //     {
        //         var events = DockManager.Instance.alertNewContentEvents;
        //         if (events == null || events.Count == 0)
        //         {
        //             text.text = "0";
        //             return;
        //         }
        //
        //         var count = 0;
        //         foreach (var e in events)
        //             if (e.LocationId == _locationId && e.DockId == _dockId)
        //                 count++;
        //
        //         text.text = count.ToString();
        //     }
        // }

        public void Initialize(string locationId, string dockId)
        {
            _locationId = locationId;
            _dockId = dockId;
        }
    }
}
