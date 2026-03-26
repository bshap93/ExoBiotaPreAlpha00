using System;
using System.Collections.Generic;
using FirstPersonPlayer.Interactable.Gated;
using FirstPersonPlayer.Interactable.Stateful;
using Helpers.Events;
using Helpers.Events.Machine;
using Helpers.Interfaces;
using LevelConstruct.Interactable.ItemInteractables;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.SceneManagers
{
    public class MachineStateManager : MonoBehaviour, ICoreGameService,
        MMEventListener<MachineStateEvent>, MMEventListener<ElevatorStateEvent>, MMEventListener<ActionConsoleEvent>
    {
        public bool autoSave;
        Dictionary<string, bool> _actionConsoleShouldHailPlayer = new(StringComparer.Ordinal);
        Dictionary<string, ActionConsole.ActionConsoleState> _consoleStates = new(StringComparer.Ordinal);

        bool _dirty;

        Dictionary<string, StatefulElevator.ElevatorState> _elevatorStates = new(StringComparer.Ordinal);

        Dictionary<string, InteractableMachine.MachineState> _machineStates = new(StringComparer.Ordinal);

        string _savePath;
        public static MachineStateManager Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[DestructableManager] No save file found; starting with defaults.");
                Reset();
            }

            Load();
        }
        void OnEnable()
        {
            this.MMEventStartListening<MachineStateEvent>();
            this.MMEventStartListening<ElevatorStateEvent>();
            this.MMEventStartListening<ActionConsoleEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MachineStateEvent>();
            this.MMEventStopListening<ElevatorStateEvent>();
            this.MMEventStopListening<ActionConsoleEvent>();
        }
        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("MachineStates", _machineStates, path);
            ES3.Save("ConsoleStates", _consoleStates, path);
            ES3.Save("ActionConsoleHasHailedPlayer", _actionConsoleShouldHailPlayer, path);
            ES3.Save("ElevatorStates", _elevatorStates, path);
            _dirty = false;
        }
        public void Load()
        {
            var path = GetSaveFilePath();
            if (ES3.KeyExists("MachineStates", path))
                _machineStates = ES3.Load<Dictionary<string, InteractableMachine.MachineState>>("MachineStates", path);

            if (ES3.KeyExists("ConsoleStates", path))
                _consoleStates = ES3.Load<Dictionary<string, ActionConsole.ActionConsoleState>>("ConsoleStates", path);

            if (ES3.KeyExists("ElevatorStates", path))
                _elevatorStates = ES3.Load<Dictionary<string, StatefulElevator.ElevatorState>>("ElevatorStates", path);

            if (ES3.KeyExists("ActionConsoleHasHailedPlayer", path))
                _actionConsoleShouldHailPlayer =
                    ES3.Load<Dictionary<string, bool>>("ActionConsoleHasHailedPlayer", path);

            _dirty = false;
        }
        public void Reset()
        {
            _machineStates.Clear();
            _consoleStates.Clear();
            _elevatorStates.Clear();
            _actionConsoleShouldHailPlayer.Clear();
            _dirty = true;
            ConditionalSave();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.MachineStateSave);
        }
        public void CommitCheckpointSave()
        {
            if (!_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(ActionConsoleEvent eventType)
        {
            if (eventType.EventType == ActionConsoleEventType.RequestActionConsoleHailsPlayer)
                if (_actionConsoleShouldHailPlayer[eventType.UniqueID])
                    SpontaneousTriggerEvent.Trigger(
                        eventType.UniqueID,
                        SpontaneousTriggerEventType.Triggered);
        }
        public void OnMMEvent(ElevatorStateEvent eventType)
        {
            if (eventType.EventType != ElevatorStateEventType.SetNewFloorState) return;
            _elevatorStates[eventType.UniqueID] = eventType.State;

            MarkDirty();
            ConditionalSave();
        }
        public void OnMMEvent(MachineStateEvent eventType)
        {
            _machineStates[eventType.UniqueID] = eventType.State;
            MarkDirty();
            ConditionalSave();
        }
        public InteractableMachine.MachineState GetMachineStateByID(string machineTypeMachineID)
        {
            if (_machineStates.Count <= 0) return InteractableMachine.MachineState.None;

            foreach (var key in _machineStates.Keys)
                if (key == machineTypeMachineID)
                {
                    _machineStates.TryGetValue(machineTypeMachineID, out var state);
                    return state;
                }

            return InteractableMachine.MachineState.None;
        }
        public ActionConsole.ActionConsoleState GetConsoleStateByID(string uniqueID)
        {
            _consoleStates.TryGetValue(uniqueID, out var state);
            return state;
        }

        public void SetIfActionConsoleShouldHailPlayer(string uniqueID, bool shouldHail)
        {
            _actionConsoleShouldHailPlayer[uniqueID] = shouldHail;
            MarkDirty();
            ConditionalSave();
        }

        public bool GetActionConsoleShouldHailPlayer(string uniqueID)
        {
            _actionConsoleShouldHailPlayer.TryGetValue(uniqueID, out var shouldHail);
            return shouldHail;
        }
    }
}
