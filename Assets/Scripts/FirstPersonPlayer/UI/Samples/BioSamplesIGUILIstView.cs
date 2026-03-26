using System.Collections.Generic;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Manager.Global;
using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.UI.Samples
{
    public class BioSamplesIGUILIstView : MonoBehaviour, MMEventListener<BioSampleEvent>
    {
        [SerializeField] Transform listTransform;
        [SerializeField] GameObject samplesListViewElementPrefab;

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(BioSampleEvent eventType)
        {
            if (eventType.EventType == BioSampleEventType.CompleteCollection ||
                eventType.EventType == BioSampleEventType.CompletedSequencing ||
                eventType.EventType == BioSampleEventType.RefreshUI)
                Refresh();
        }

        public void Refresh()
        {
            var bioMgr = BioSamplesManager.Instance;
            if (bioMgr == null) return;

            foreach (Transform child in listTransform) Destroy(child.gameObject);

            // foreach (var sample in bioMgr.GetSamplesCarried())
            // {
            //     var go = Instantiate(samplesListViewElementPrefab, listTransform);
            //     var element = go.GetComponent<SamplesListViewElement>();
            //     if (element != null)
            //         element.Bind(sample); // <-- Standard row only
            // }
            // Group samples by organism ID; keep the first sample as the representative
            var grouped = new Dictionary<string, (BioOrganismSample representative, int count)>();
            foreach (var sample in bioMgr.GetSamplesCarried())
            {
                var key = sample.parentOrganismID;
                if (grouped.TryGetValue(key, out var existing))
                    grouped[key] = (existing.representative, existing.count + 1);
                else
                    grouped[key] = (sample, 1);
            }

            foreach (var (_, (representative, count)) in grouped)
            {
                var go = Instantiate(samplesListViewElementPrefab, listTransform);
                var element = go.GetComponent<SamplesListViewElement>();
                if (element != null)
                    element.Bind(representative, count);
            }
        }
    }
}
