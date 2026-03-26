using System;
using System.Collections.Generic;
using Helpers.Events.Triggering;
using MoreMountains.Tools;
using PhysicsHandlers.Triggers;

namespace Manager
{
    public class TriggerColliderManager : GameManagerAbstract<TriggerColliderManager>,
        MMEventListener<TriggerColliderEvent>
    {
        Dictionary<string, bool> _dialogueCollidersTriggerable = new(StringComparer.Ordinal);
        Dictionary<string, bool> _objectiveCollidersTriggerable = new(StringComparer.Ordinal);
        Dictionary<string, bool> _spontaneousColliersTriggerable = new(StringComparer.Ordinal);
        Dictionary<string, bool> _tutorialCollidersTriggerable = new(StringComparer.Ordinal);
        public override void Reset()
        {
            _spontaneousColliersTriggerable.Clear();
            _tutorialCollidersTriggerable.Clear();
            _objectiveCollidersTriggerable.Clear();
            _dialogueCollidersTriggerable.Clear();
            base.Reset();
        }

        protected override void OnEnable()
        {
            this.MMEventStartListening();
        }
        protected override void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(TriggerColliderEvent eventType)
        {
            if (eventType.EventType == TriggerColliderEventType.SetTriggerable)
            {
                if (eventType.ColliderType == TriggerColliderType.Spontaneous)
                    SetSpontaneousColliderTriggerable(eventType.ColliderID, eventType.IsTriggerable);
                else if (eventType.ColliderType == TriggerColliderType.Tutorial)
                    SetTutorialColliderTriggerable(eventType.ColliderID, eventType.IsTriggerable);
                else if (eventType.ColliderType == TriggerColliderType.Objective)
                    SetObjectiveColliderTriggerable(eventType.ColliderID, eventType.IsTriggerable);
                else if (eventType.ColliderType == TriggerColliderType.Dialogue)
                    SetDialogueColliderTriggerable(eventType.ColliderID, eventType.IsTriggerable);
            }
        }
        public override string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.TriggerColliderSave);
        }

        public override void Save()
        {
            var path = GetSaveFilePath();

            ES3.Save("SpontaneousCollidersTriggerable", _spontaneousColliersTriggerable, path);
            ES3.Save("TutorialCollidersTriggerable", _tutorialCollidersTriggerable, path);
            ES3.Save("ObjectiveCollidersTriggerable", _objectiveCollidersTriggerable, path);
            ES3.Save("DialogueCollidersTriggerable", _dialogueCollidersTriggerable, path);
            Dirty = false;
        }

        public override void Load()
        {
            var path = GetSaveFilePath();
            if (ES3.KeyExists("SpontaneousCollidersTriggerable", path))
                _spontaneousColliersTriggerable =
                    ES3.Load<Dictionary<string, bool>>("SpontaneousCollidersTriggerable", path);

            if (ES3.KeyExists("TutorialCollidersTriggerable", path))
                _tutorialCollidersTriggerable =
                    ES3.Load<Dictionary<string, bool>>("TutorialCollidersTriggerable", path);

            if (ES3.KeyExists("ObjectiveCollidersTriggerable", path))
                _objectiveCollidersTriggerable =
                    ES3.Load<Dictionary<string, bool>>("ObjectiveCollidersTriggerable", path);

            if (ES3.KeyExists("DialogueCollidersTriggerable", path))
                _dialogueCollidersTriggerable =
                    ES3.Load<Dictionary<string, bool>>("DialogueCollidersTriggerable", path);

            Dirty = false;
        }

        public void SetSpontaneousColliderTriggerable(string colliderID, bool isTriggerable)
        {
            _spontaneousColliersTriggerable[colliderID] = isTriggerable;
            Dirty = true;
        }

        public void SetTutorialColliderTriggerable(string colliderID, bool isTriggerable)
        {
            _tutorialCollidersTriggerable[colliderID] = isTriggerable;
            Dirty = true;
        }

        public void SetObjectiveColliderTriggerable(string colliderID, bool isTriggerable)
        {
            _objectiveCollidersTriggerable[colliderID] = isTriggerable;
            Dirty = true;
        }

        public bool IsSpontaneousColliderTriggerable(string colliderID)
        {
            // Default to true. Easier to see if it's pushing up when it shouldn't be than the opposite.
            return _spontaneousColliersTriggerable.GetValueOrDefault(colliderID, true);
        }

        public bool IsTutorialColliderTriggerable(string colliderID)
        {
            // Default to true. Easier to see if it's pushing up when it shouldn't be than the opposite.
            return _tutorialCollidersTriggerable.GetValueOrDefault(colliderID, true);
        }

        public bool IsObjectiveColliderTriggerable(string colliderID)
        {
            // Default to true. Easier to see if it's pushing up when it shouldn't be than the opposite.
            return _objectiveCollidersTriggerable.GetValueOrDefault(colliderID, true);
        }

        public void SetDialogueColliderTriggerable(string colliderID, bool isTriggerable)
        {
            _dialogueCollidersTriggerable[colliderID] = isTriggerable;
            Dirty = true;
        }
        public bool IsDialogueColliderTriggerable(string uniqueID)
        {
            // Default to true. Easier to see if it's pushing up when it shouldn't be than the opposite.
            return _dialogueCollidersTriggerable.GetValueOrDefault(uniqueID, true);
        }
    }
}
