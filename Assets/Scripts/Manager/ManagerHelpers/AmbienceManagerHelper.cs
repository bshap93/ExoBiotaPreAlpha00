using AmbientSounds;
using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.ManagerHelpers
{
    public class AmbienceManagerHelper : MonoBehaviour, MMEventListener<SceneEvent>
    {
        [FormerlySerializedAs("_ambienceManager")] [SerializeField]
        AmbienceManager ambienceManager;
        bool _isPaused;

        void Awake()
        {
            if (ambienceManager == null) ambienceManager = GetComponent<AmbienceManager>();
            if (ambienceManager == null) Debug.LogError("AmbienceManager not found in the scene.");

            _isPaused = false;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(SceneEvent eventType)
        {
            if (eventType.EventType == SceneEventType.TogglePauseScene)
            {
                if (!_isPaused)
                {
                    AmbienceManager.PausePlayback();

                    _isPaused = true;
                }
                else
                {
                    AmbienceManager.ContinuePlayback();
                    _isPaused = false;
                }
            }
        }
    }
}
