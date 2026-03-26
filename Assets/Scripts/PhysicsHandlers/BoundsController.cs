using UnityEngine;
using UnityEngine.Serialization;

namespace PhysicsHandlers
{
    public class BoundsController : MonoBehaviour
    {
        [FormerlySerializedAs("startingFPAreaBounds")]
        public GameObject fpAreaBounds;

        public GameObject dirigibleAreaBounds;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
        }
    }
}