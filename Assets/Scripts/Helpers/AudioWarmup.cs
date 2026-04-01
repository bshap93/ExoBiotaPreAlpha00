using UnityEngine;

namespace Helpers
{
    public class AudioWarmup : MonoBehaviour
    {
        void Awake()
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.volume = 0f;
            source.playOnAwake = false;

            // Use any tiny clip you have, or create a silent one
            source.Play();
            Destroy(source, 1f);
        }
    }
}
