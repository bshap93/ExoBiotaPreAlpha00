using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class InGameTimeManager : MonoBehaviour, ICoreGameService, MMEventListener<InGameTimeActionEvent>,
        MMEventListener<FirstPersonDialogueEvent>, MMEventListener<MyUIEvent>

    {
        public enum TimeState
        {
            RunningNormalSpeed,
            Paused,
            Accelerated
        }


        public bool autoSave;

        public TimeState timeState = TimeState.RunningNormalSpeed;

        [Header("Event Debouncing")]
        [Tooltip("Minimum time in seconds between processing identical time action events")]
        [SerializeField]
        float actionDebounceTime = 0.5f;

        [Header("Time Settings")] [SerializeField]
        int minutesPerDay = 800;
        [Range(1f, 3f)] [SerializeField]
        float secondsRealTimePerInGameMinute = 2f; // Day length is 1600 seconds or ~26.67 minutes
        [Range(0, 800)] [SerializeField] int minutesIntoDayAtStart = 400; // midday start
        public
            int orbitalPeriodInDays = 16;
        [SerializeField] int initialDaysElapsed;

        float _accumulatedTime;

        int _currentMinuteOfDay;
        bool _dirty;
        int _inGameDaysElapsed;
        int _inGameMinutesElapsed;
        float _lastActionTime = -999f;
        InGameTimeActionEvent.ActionType _lastActionType = InGameTimeActionEvent.ActionType.Resume;

        string _savePath;

        float _targetAcceleration = 1f; // Current acceleration target
        public static InGameTimeManager Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Update()
        {
            if (timeState == TimeState.RunningNormalSpeed)
            {
                // Accumulate real time
                _accumulatedTime += Time.deltaTime;

                // Convert to in-game minutes
                var minutesToAdd = Mathf.FloorToInt(_accumulatedTime / secondsRealTimePerInGameMinute);

                if (minutesToAdd > 0)
                {
                    _accumulatedTime -= minutesToAdd * secondsRealTimePerInGameMinute;
                    _currentMinuteOfDay += minutesToAdd;
                    _inGameMinutesElapsed += minutesToAdd;

                    // Handle day rollover
                    if (_currentMinuteOfDay >= minutesPerDay)
                    {
                        _inGameDaysElapsed += _currentMinuteOfDay / minutesPerDay;
                        _currentMinuteOfDay %= minutesPerDay;
                    }

                    MarkDirty();
                    InGameTimeUpdateEvent.Trigger(_currentMinuteOfDay, _inGameDaysElapsed, _inGameMinutesElapsed);
                }
            }
            else if (timeState == TimeState.Paused)
            {
                // Do nothing
            }
            else if (timeState == TimeState.Accelerated)
            {
                // ✅ Use the calculated target acceleration
                _accumulatedTime += Time.deltaTime * _targetAcceleration;

                var minutesToAdd = Mathf.FloorToInt(_accumulatedTime / secondsRealTimePerInGameMinute);

                if (minutesToAdd > 0)
                {
                    _accumulatedTime -= minutesToAdd * secondsRealTimePerInGameMinute;
                    _currentMinuteOfDay += minutesToAdd;
                    _inGameMinutesElapsed += minutesToAdd;

                    if (_currentMinuteOfDay >= minutesPerDay)
                    {
                        _inGameDaysElapsed += _currentMinuteOfDay / minutesPerDay;
                        _currentMinuteOfDay %= minutesPerDay;
                    }

                    MarkDirty();
                    InGameTimeUpdateEvent.Trigger(_currentMinuteOfDay, _inGameDaysElapsed, _inGameMinutesElapsed);
                }
            }
        }


        void OnEnable()
        {
            this.MMEventStartListening<InGameTimeActionEvent>();
            this.MMEventStartListening<FirstPersonDialogueEvent>();
            this.MMEventStartListening<MyUIEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<InGameTimeActionEvent>();
            this.MMEventStopListening<FirstPersonDialogueEvent>();
            this.MMEventStopListening<MyUIEvent>();
        }
        public void Save()
        {
            _savePath = GetSaveFilePath();

            ES3.Save("inGameDaysElapsed", _inGameDaysElapsed, _savePath);
            ES3.Save("currentMinuteOfDay", _currentMinuteOfDay, _savePath);
            ES3.Save("inGameMinutesElapsed", _inGameMinutesElapsed, _savePath);
        }
        public void Load()
        {
            _savePath = GetSaveFilePath();
            if (ES3.KeyExists("inGameDaysElapsed", _savePath))
                _inGameDaysElapsed = ES3.Load<int>("inGameDaysElapsed", _savePath);
            else _inGameDaysElapsed = 0;

            if (ES3.KeyExists("currentMinuteOfDay", _savePath))
                _currentMinuteOfDay = ES3.Load<int>("currentMinuteOfDay", _savePath);
            else _currentMinuteOfDay = minutesIntoDayAtStart;

            if (ES3.KeyExists("inGameMinutesElapsed", _savePath))
                _inGameMinutesElapsed = ES3.Load<int>("inGameMinutesElapsed", _savePath);
            else _inGameMinutesElapsed = 0;
        }
        public void Reset()
        {
            _inGameDaysElapsed = initialDaysElapsed;
            _currentMinuteOfDay = minutesIntoDayAtStart;
            _inGameMinutesElapsed = initialDaysElapsed * minutesPerDay;
            Save();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.InGameTimeSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty)
            {
                Save();
                _dirty = false;
            }
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(FirstPersonDialogueEvent eventType)
        {
            if (eventType.Type == FirstPersonDialogueEventType.StartDialogue)
                timeState = TimeState.Paused;
            else if (eventType.Type == FirstPersonDialogueEventType.EndDialogue)
                timeState = TimeState.RunningNormalSpeed;
        }
        public void OnMMEvent(InGameTimeActionEvent eventType)
        {
            var isSameAction = eventType.ActionTypeIG == _lastActionType;
            var withinDebounceWindow = Time.time - _lastActionTime < actionDebounceTime;

            if (isSameAction && withinDebounceWindow)
                // Silently ignore duplicate events
                return;

            // Update tracking
            _lastActionType = eventType.ActionTypeIG;
            _lastActionTime = Time.time;

            Debug.Log(eventType + " received by InGameTimeManager");

            switch (eventType.ActionTypeIG)
            {
                case InGameTimeActionEvent.ActionType.Pause:
                    timeState = TimeState.Paused;
                    break;
                case InGameTimeActionEvent.ActionType.Resume:
                    timeState = TimeState.RunningNormalSpeed;
                    break;
                case InGameTimeActionEvent.ActionType.LapseTime:
                    timeState = TimeState.Accelerated;
                    break;
                case InGameTimeActionEvent.ActionType.StopLapseTime:
                    timeState = TimeState.RunningNormalSpeed;
                    break;
            }
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.InfoLogTablet)
            {
                if (eventType.uiActionType == UIActionType.Open)
                    timeState = TimeState.Paused;
                else if (eventType.uiActionType == UIActionType.Close)
                    timeState = TimeState.RunningNormalSpeed;
            }
        }

        // How fast do we need to accelerate to advance X in-game minutes in Y real seconds?
        public float CalculateRequiredAcceleration(int inGameMinutes, float realWorldSeconds)
        {
            // Normal time: inGameMinutes would take (inGameMinutes * secondsRealTimePerInGameMinute) real seconds
            // We want it to happen in realWorldSeconds instead
            // So acceleration = normalDuration / desiredDuration
            var normalDuration = inGameMinutes * secondsRealTimePerInGameMinute;
            return normalDuration / realWorldSeconds;
        }

        public void SetAcceleration(float multiplier)
        {
            _targetAcceleration = multiplier;
        }
    }
}
