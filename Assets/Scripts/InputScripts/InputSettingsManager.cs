using Domains.Input.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace InputScripts
{
    public class InputSettingsManager : MonoBehaviour, MMEventListener<InputSettingsEvent>
    {
        [SerializeField] bool invertYAxis;
        [SerializeField] float mouseSensitivity = 1f;
        [SerializeField] bool showKeyboardControls = true;
        public static InputSettingsManager Instance { get; private set; }


        public bool InvertYAxis
        {
            get => invertYAxis;
            set
            {
                invertYAxis = value;
                PlayerPrefs.SetInt("InvertYAxis", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public bool ShowKeyboardControls
        {
            get => showKeyboardControls;
            set
            {
                showKeyboardControls = value;
                PlayerPrefs.SetInt("ShowKeyboardControls", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public float MouseSensitivity
        {
            get => PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            set
            {
                PlayerPrefs.SetFloat("MouseSensitivity", value);
                PlayerPrefs.Save();
            }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // Load saved setting
            if (PlayerPrefs.HasKey("InvertYAxis"))
                invertYAxis = PlayerPrefs.GetInt("InvertYAxis") == 1;

            if (PlayerPrefs.HasKey("MouseSensitivity"))
                mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");

            if (PlayerPrefs.HasKey("ShowKeyboardControls"))
                showKeyboardControls = PlayerPrefs.GetInt("ShowKeyboardControls") == 1;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(InputSettingsEvent eventType)
        {
            switch (eventType.EventType)
            {
                case InputSettingsEventType.InvertYAxis:
                    if (eventType.BoolValue != null) InvertYAxis = eventType.BoolValue.Value;
                    break;
                case InputSettingsEventType.SetMouseSensitivity:
                    if (eventType.FloatValue != null) MouseSensitivity = eventType.FloatValue.Value;
                    break;
                case InputSettingsEventType.ShowKeyboardControls:
                    if (eventType.BoolValue != null) ShowKeyboardControls = eventType.BoolValue.Value;
                    break;
            }
        }
    }
}
