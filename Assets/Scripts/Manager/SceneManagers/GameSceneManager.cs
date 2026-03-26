using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager.SceneManagers
{
    public class GameSceneManager : MonoBehaviour, MMEventListener<SceneEvent>
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
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
            if (eventType.EventType == SceneEventType.PlayerRequestsQuit)
                // Debug.Log("Scene Loaded Event Received");
                Application.Quit();
            else if (eventType.EventType == SceneEventType.PlayerRequestsMainMenu)
                // Debug.Log("Scene Loaded Event Received");
                SceneManager.LoadScene("Scenes/TitleScreen");
        }
        public bool GameHasDataToContinue()
        {
            return true;
        }
    }
}
