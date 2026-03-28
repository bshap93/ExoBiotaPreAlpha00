using UnityEngine;

namespace Helpers
{
    public class DestroyEffectAfterDelay : MonoBehaviour
    {
        public float delay;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Destroy(gameObject, delay);
        }
    }
}
