using System;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewScript
{
    public class ObjectiveHelper : MonoBehaviour, MMEventListener<ObjectiveEvent>
    {
        public ObjectiveObject associatedObjective;
        ObjectiveHelperType _helperType;

        ItemPicker _itemPicker;

        void Start()
        {
            if (_helperType == ObjectiveHelperType.ItemPicker)
                // Try to get ItemPicker component
                _itemPicker = GetComponent<ItemPicker>();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ObjectiveEvent eventType)
        {
            if (eventType.type == ObjectiveEventType.ObjectiveActivated)
                if (eventType.objectiveId == associatedObjective.objectiveId)
                    if (eventType.locationObject != null)
                        if (eventType.locationObject.associatedPOIUniqueId != null)
                        {
                            var sceneName = SceneManager.GetActiveScene().name;
                            GamePOIEvent.Trigger(
                                eventType.locationObject.associatedPOIUniqueId,
                                GamePOIEventType.MarkPOIAsTrackedByObjective,
                                sceneName);
                        }
        }

        public void CompleteObjective()
        {
            if (associatedObjective == null)
            {
                Debug.LogWarning("No associated objective to complete.");
                return;
            }

            ObjectiveEvent.Trigger(
                associatedObjective.objectiveId, ObjectiveEventType.ObjectiveCompleted
            );
        }

        public void ProgressObjectiveByN(int n)
        {
            if (associatedObjective == null)
            {
                Debug.LogWarning("No associated objective to progress.");
                return;
            }

            ObjectiveEvent.Trigger(
                associatedObjective.objectiveId, ObjectiveEventType.IncrementObjectiveProgress,
                NotifyType.Regular, n);
        }

        [Serializable]
        enum ObjectiveHelperType
        {
            ItemPicker,
            SampleableOrganism
        }
    }
}
