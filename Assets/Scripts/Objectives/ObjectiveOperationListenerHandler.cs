using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.Events.Status;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine;

namespace Objectives
{
    public class ObjectiveOperationListenerHandler : MonoBehaviour, MMEventListener<ObjectiveEvent>,
        MMEventListener<GatedLevelingEvent>, MMEventListener<PlayerStatsEvent>
    {
        [SerializeField] ObjectiveObject objectiveToCompleteWhenGatedLevelingEventOccurs;
        [SerializeField] ObjectiveObject objectiveToCompleteWhenDecontaminationTakesPlace;

        ObjectivesManager _objectivesManager;

        void Awake()
        {
            _objectivesManager = GetComponent<ObjectivesManager>();
        }
        void OnEnable()
        {
            this.MMEventStartListening<ObjectiveEvent>();
            this.MMEventStartListening<GatedLevelingEvent>();
            this.MMEventStartListening<PlayerStatsEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<ObjectiveEvent>();
            this.MMEventStopListening<GatedLevelingEvent>();
            this.MMEventStopListening<PlayerStatsEvent>();
        }
        public void OnMMEvent(GatedLevelingEvent eventType)
        {
            if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
                _objectivesManager.CompleteObjective(objectiveToCompleteWhenGatedLevelingEventOccurs.objectiveId);
        }


        public void OnMMEvent(ObjectiveEvent e)
        {
        }
        public void OnMMEvent(PlayerStatsEvent eventType)
        {
            if (eventType.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease &&
                eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentContamination)
                _objectivesManager.CompleteObjective(objectiveToCompleteWhenDecontaminationTakesPlace.objectiveId);
        }
    }
}
