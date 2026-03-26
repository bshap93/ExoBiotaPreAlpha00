using System.Collections;
using System.Collections.Generic;
using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Objectives.UI
{
    public class ObjectiveDisplayer : MonoBehaviour, MMEventListener<ObjectiveEvent>
    {
        [Tooltip("How long the toast stays on screen")]
        public float DisplayDuration = 4f;

        [Tooltip("Fade in/out duration")] public float FadeDuration = 0.2f;

        [Tooltip("Ignore early events for this many seconds after Start (prevents load spam)")]
        public float IgnoreEarlyObjectiveEventsSeconds = 0.5f;

        [Tooltip("Prefab that contains the UI for a single objective toast")]
        public ObjectiveDisplayItem DisplayPrefab;

        readonly Dictionary<string, ObjectiveDisplayItem> _activeToasts = new();
        WaitForSeconds _displayWfs;
        bool _listening;

        IEnumerator Start()
        {
            // Delay to avoid the manager's initial re-broadcast flooding the UI
            yield return new WaitForSeconds(IgnoreEarlyObjectiveEventsSeconds);
            this.MMEventStartListening();
            _listening = true;
            if (_displayWfs == null) _displayWfs = new WaitForSeconds(DisplayDuration);
        }

        void OnDisable()
        {
            if (_listening)
            {
                this.MMEventStopListening();
                _listening = false;
            }
        }

        void OnValidate()
        {
            _displayWfs = new WaitForSeconds(DisplayDuration);
        }

        public void OnMMEvent(ObjectiveEvent e)
        {
            // Show toast on activation and/or completion
            if (e.type != ObjectiveEventType.ObjectiveActivated &&
                e.type != ObjectiveEventType.ObjectiveCompleted)
                return;

            var mgr = ObjectivesManager.Instance;
            if (mgr == null) return;

            var obj = mgr.GetObjectiveById(e.objectiveId);
            if (obj == null) return;

            var status = e.type == ObjectiveEventType.ObjectiveActivated ? "New Objective" : "Objective Complete";

            if (_activeToasts.TryGetValue(obj.objectiveId, out var existing))
            {
                // If a toast for this objective already exists, just bump the status text
                existing.SetStatus(status);
            }
            else
            {
                var item = Instantiate(DisplayPrefab, transform);
                _activeToasts[obj.objectiveId] = item;

                item.Display(obj, status);

                var cg = item.GetComponent<CanvasGroup>();
                if (cg)
                {
                    cg.alpha = 0;
                    StartCoroutine(MMFade.FadeCanvasGroup(cg, FadeDuration, 1));
                }

                StartCoroutine(FadeOutAndDestroy(obj.objectiveId, item, cg));
            }
        }

        IEnumerator FadeOutAndDestroy(string key, ObjectiveDisplayItem item, CanvasGroup cg)
        {
            yield return _displayWfs;
            if (cg) yield return MMFade.FadeCanvasGroup(cg, FadeDuration, 0);
            if (item) Destroy(item.gameObject);
            _activeToasts.Remove(key);
        }
    }
}
