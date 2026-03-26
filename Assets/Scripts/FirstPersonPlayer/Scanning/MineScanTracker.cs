using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanning
{
    public class MineScanTracker : MonoBehaviour
    {
        [Header("Where to look for section POIs")] [SerializeField]
        private Transform searchRoot; // optional; if null, searches whole scene

        [SerializeField] private bool autoCollectOnStart = true;

        [Header("Debug / HUD")] [SerializeField]
        private bool logPercentChanges = true;

        private readonly HashSet<string> _discovered = new();

        private List<MineSectionPOI> _allSections = new();

        public float PercentMapped =>
            _allSections.Count == 0 ? 0f : (float)_discovered.Count / _allSections.Count;

        private void Start()
        {
            if (autoCollectOnStart) CollectSections();
            Notify();
        }

        public event Action<float> OnPercentChanged;

        public void CollectSections()
        {
            _allSections.Clear();
            if (searchRoot != null)
                _allSections.AddRange(searchRoot.GetComponentsInChildren<MineSectionPOI>(true));
            else
                _allSections.AddRange(
                    FindObjectsByType<MineSectionPOI>(FindObjectsInactive.Include, FindObjectsSortMode.None));

            // de-dupe by sectionId (in case of duplicates)
            _allSections = _allSections
                .Where(s => !string.IsNullOrWhiteSpace(s.sectionId))
                .GroupBy(s => s.sectionId)
                .Select(g => g.First()).ToList();
        }

        public void HandleScanHit(Transform hitTransform)
        {
            if (hitTransform == null) return;

            // look on self or parents for a MineSectionPOI
            var marker = hitTransform.GetComponentInParent<MineSectionPOI>();
            if (marker == null || string.IsNullOrWhiteSpace(marker.sectionId)) return;

            if (_discovered.Add(marker.sectionId)) Notify();
        }

        public void ResetProgress()
        {
            _discovered.Clear();
            Notify();
        }

        public void Notify()
        {
            var p = PercentMapped;
            if (logPercentChanges)
                Debug.Log($"[MineScanTracker] Mapped: {p * 100f:0.#}% ({_discovered.Count}/{_allSections.Count})");
            OnPercentChanged?.Invoke(p);
        }
    }
}