using MoreMountains.Feedbacks;
using UnityEngine;

namespace Feedbacks
{
    public class RigidOrganismListener : MonoBehaviour
    {
        [SerializeField] MMFeedbacks hitFeedbacks;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("FirstPersonPlayer"))
            {
                hitFeedbacks?.PlayFeedbacks();
                Debug.Log("Hit " + other.gameObject.name);
            }
        }
    }
}
