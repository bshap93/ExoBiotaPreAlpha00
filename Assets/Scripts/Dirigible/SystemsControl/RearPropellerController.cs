using Dirigible.Controllers;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dirigible.SystemsControl
{
    public class RearPropellerController : MonoBehaviour
    {
        [FormerlySerializedAs("PropellerBlade")]
        public GameObject propellerBlade;

        public MMFeedbacks rearPropellerMainSoundFeedbacks;


        [Header("Spin Settings")] public float minRPM = 50f; // idle

        public float maxRPM = 2000f; // full throttle

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("Response")] public float rpmChangeSpeed = 2f; // How quickly the RPM changes

        [SerializeField] private float currentRPM;


        public void UpdatePropeller(DirigibleStatus status)
        {
            // Only use positive forward thrust — backward or zero means prop stops
            var normalizedThrust = Mathf.Clamp01(status.currentThrust / 1500f); // 0 to 1 only for forward

            var targetRPM = normalizedThrust > 0f
                ? Mathf.Lerp(minRPM, maxRPM, normalizedThrust)
                : 0f; // Fully stop when idle or in reverse

            // Smoothly adjust RPM
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime * rpmChangeSpeed);

            // Spin the propeller blade
            var degreesPerSecond = currentRPM / 60f * 360f;
            propellerBlade.transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}