using System.Collections;
using Helpers.Events.Triggering;
using Helpers.Events.UI;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Helpers.Collider
{
    public class SceneAdditiveTrigger : MonoBehaviour, MMEventListener<MySceneTransitionAdditiveEvent>
    {
        [SerializeField] string sceneToLoad;
        [SerializeField] bool setActiveOnLoad = true;

        [FormerlySerializedAs("triggerOnPlayerLocation")] [SerializeField]
        bool triggerLoadOnPlayerLocation;
        [SerializeField] bool triggerUnloadOnPlayerLocation;
        [SerializeField] bool setActiveSceneOnPlayerLocation;
        Coroutine _loadCoroutine;

        bool _sceneLoaded;
        bool _sceneUnloaded;

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (setActiveSceneOnPlayerLocation)
            {
                if (!other.CompareTag("FirstPersonPlayer")) return;

                SetSceneAsActiveScene();
            }

            if (!triggerLoadOnPlayerLocation) return;
            if (!other.CompareTag("FirstPersonPlayer")) return;

            LoadScene();
        }

        void OnTriggerExit(UnityEngine.Collider other)
        {
            if (!triggerUnloadOnPlayerLocation) return;
            if (!other.CompareTag("FirstPersonPlayer")) return;

            Debug.Log($"Player exited trigger for scene '{sceneToLoad}'");


            UnloadScene();
        }
        public void OnMMEvent(MySceneTransitionAdditiveEvent eventType)
        {
            if (eventType.SceneName != sceneToLoad) return;

            if (eventType.EventType == MySceneTransitionAdditiveEvent.MySceneTransEventType.Load)
                LoadScene();
            else if (eventType.EventType == MySceneTransitionAdditiveEvent.MySceneTransEventType.Unload) UnloadScene();
        }
        void LoadScene()
        {
            if (_sceneLoaded || _loadCoroutine != null) return;
            var scene = SceneManager.GetSceneByName(sceneToLoad);
            if (scene.isLoaded)
            {
                _sceneLoaded = true;
                _sceneUnloaded = false;
                if (setActiveOnLoad) SceneManager.SetActiveScene(scene);
                return;
            }

            SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);

            _loadCoroutine = StartCoroutine(LoadSceneAsync());
        }
        void UnloadScene()
        {
            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
                _loadCoroutine = null;
            }

            if (_sceneUnloaded) return;

            SceneManager.UnloadSceneAsync(sceneToLoad);
            _sceneLoaded = false;
            _sceneUnloaded = true;
            Debug.Log($"Unloaded scene '{sceneToLoad}'");
        }

        IEnumerator LoadSceneAsync()
        {
            Debug.Log($"Loading scene '{sceneToLoad}'...");

            var op = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

            // Wait until Unity reports the scene is fully loaded
            yield return new WaitUntil(() => op != null && op.isDone);

            _sceneLoaded = true;
            _sceneUnloaded = false;
            _loadCoroutine = null;

            SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Hide);

            SetSceneAsActiveScene();
        }
        void SetSceneAsActiveScene()
        {
            var scene = SceneManager.GetSceneByName(sceneToLoad);
            Debug.Log($"Loaded scene '{sceneToLoad}'");

            if (setActiveOnLoad && scene.IsValid() && scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
                Debug.Log($"Active scene set to '{sceneToLoad}'");
            }
        }
    }
}
