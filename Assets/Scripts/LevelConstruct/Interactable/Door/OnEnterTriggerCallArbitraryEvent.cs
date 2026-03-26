using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelConstruct.Interactable.Door
{
    public class OnEnterTriggerCallArbitraryEvent : MonoBehaviour
    {
        [FormerlySerializedAs("OnTriggerEnterEvent")]
        public UnityEvent onTriggerEnterEvent;
        public UnityEvent onTriggerExitEvent;
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer"))
                onTriggerEnterEvent?.Invoke();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer"))
                onTriggerExitEvent?.Invoke();
        }
    }
}
