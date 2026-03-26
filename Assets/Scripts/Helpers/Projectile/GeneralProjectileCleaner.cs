using UnityEngine;

namespace Helpers.Projectile
{
    public class GeneralProjectileCleaner : MonoBehaviour
    {
        public float lifetime = 5f;

        void Awake()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
