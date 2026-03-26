using System.Collections.Generic;
using Helpers.Events;
using Michsky.MUIP;
using MoreMountains.Tools;
using Objectives;
using Objectives.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI.IGUI
{
    public class ObjectivesIGUIController : MonoBehaviour, MMEventListener<ObjectiveEvent>
    {
        [FormerlySerializedAs("objectivesDropdown")] [SerializeField]
        CustomDropdown objectivesTypeDropdown;

        [SerializeField] Transform listTransform;

        // 0 = Active, 1 = Completed, 2 = All (match your dropdown options)
        [SerializeField] int _filterIndex;

        [SerializeField] GameObject objectiveListItemPrefab;

        void OnEnable()
        {
            this.MMEventStartListening();
            Refresh();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ObjectiveEvent e)
        {
            // Any activation/completion should refresh the list if we're visible
            if (e.type == ObjectiveEventType.ObjectiveActivated ||
                e.type == ObjectiveEventType.ObjectiveDeactivated ||
                e.type == ObjectiveEventType.ObjectiveCompleted ||
                e.type == ObjectiveEventType.ObjectiveAdded)
                Refresh();
        }

        // Expose this so you can wire dropdown's OnValueChanged(int) in the Inspector
        public void SetFilter(int index)
        {
            _filterIndex = index;
            Refresh();
        }


        public void Refresh()
        {
            var mgr = ObjectivesManager.Instance;
            if (mgr == null) return;

            // Clear existing list
            foreach (Transform child in listTransform)
                Destroy(child.gameObject);

            if (objectivesTypeDropdown != null)
                _filterIndex = objectivesTypeDropdown.selectedItemIndex;

            foreach (var obj in EnumerateByFilter(mgr, _filterIndex))
            {
                if (obj == null) continue;
                var go = Instantiate(objectiveListItemPrefab, listTransform);
                var element = go.GetComponent<ObjectiveIGUIListElement>();
                element.Initialize(obj);
            }
        }

        // 0 = Active, 1 = Completed, 2 = Inactive, 3 = All (ADDED ONLY)
        static IEnumerable<ObjectiveObject> EnumerateByFilter(ObjectivesManager mgr, int filterIndex)
        {
            if (filterIndex == 0)
            {
                foreach (var id in mgr.GetActiveObjectives())
                    yield return mgr.GetObjectiveById(id);

                yield break;
            }

            if (filterIndex == 1)
            {
                foreach (var id in mgr.GetCompletedObjectives())
                    yield return mgr.GetObjectiveById(id);

                yield break;
            }

            if (filterIndex == 2)
            {
                foreach (var id in mgr.GetInactiveObjectives())
                    yield return mgr.GetObjectiveById(id);

                yield break;
            }

            // 3 = All (added only, i.e., union of the three sets)
            var seen = new HashSet<string>();
            foreach (var id in mgr.GetActiveObjectives())
                if (seen.Add(id))
                    yield return mgr.GetObjectiveById(id);

            foreach (var id in mgr.GetInactiveObjectives())
                if (seen.Add(id))
                    yield return mgr.GetObjectiveById(id);

            foreach (var id in mgr.GetCompletedObjectives())
                if (seen.Add(id))
                    yield return mgr.GetObjectiveById(id);
        }
    }
}
