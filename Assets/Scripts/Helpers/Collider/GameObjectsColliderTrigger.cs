using UnityEngine;

namespace Helpers.Collider
{
    public class GameObjectsColliderTrigger : MonoBehaviour
    {
        public GameObject[] gameObjectsToToggle;
        void Awake()
        {
            DeactivateGameObjects();
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) ActivateGameObjects();
        }

        void OnTriggerExit(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) DeactivateGameObjects();
        }

        void DeactivateGameObjects()
        {
            foreach (var gameObject0 in gameObjectsToToggle) gameObject0.SetActive(false);
        }

        void ActivateGameObjects()
        {
            foreach (var gameObject1 in gameObjectsToToggle) gameObject1.SetActive(true);
        }
    }
}
