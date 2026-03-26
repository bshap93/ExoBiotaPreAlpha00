using UnityEngine;

namespace Helpers.CleanupTempObjects
{
    public class SphereSmokeProjectileImpacts : MonoBehaviour
    {
        public float lifetime = 5f;
        float timer;

        void Start()
        {
            timer = lifetime;
        }

        // Update is called once per frame
        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) Destroy(gameObject);
        }
    }
}
