using Helpers.Events.Playtest;
using UnityEngine;

namespace NewScript.Triggere
{
    public class PlaytestColliderTrigger : MonoBehaviour
    {
        [SerializeField] PlaytestInfoLogEventType eventType;
        bool _hasTriggered;

        void OnTriggerEnter(Collider other)
        {
            if (eventType == PlaytestInfoLogEventType.Intro && !_hasTriggered)
            {
                PlaytestInfoLogEvent.Trigger(eventType);
                _hasTriggered = true;
            }
        }
    }
}
