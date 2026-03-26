using UnityEngine;

namespace Helpers.Collider
{
    public class LightsColliderTrigger : MonoBehaviour
    {
        public GameObject[] lightsToToggle;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            DeactivateLights();
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) ActivateLights();
        }

        void OnTriggerExit(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) DeactivateLights();
        }

        void DeactivateLights()
        {
            foreach (var mixLight in lightsToToggle)
                if (mixLight != null)
                    mixLight.SetActive(false);
        }

        void ActivateLights()
        {
            foreach (var mixLight in lightsToToggle)
                if (mixLight != null)
                    mixLight.SetActive(true);
        }
    }
}
