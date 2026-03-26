using Dirigible.Controllers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Dirigible.SystemsControl
{
    public class DirigibleLiftRotor : MonoBehaviour
    {
        public GameObject rotorBlade;
        public MMFeedbacks liftRotorMainSoundFeedbacks;

        [Header("Spin Settings")] public float idleRPM = 300f;

        public float mediumRPM = 800f;
        public float rpmChangeSpeed = 2f;
        public float maxRPM = 2000;

        [SerializeField] private float currentRPM;


        public void UpdateRotor(DirigibleStatus status)
        {
            // Use vertical input: changeHeightValue is -1 (descend), 0 (hover), +1 (ascend)
            var input = status.currentVerticalInput;

            float targetRPM;

            if (input > 0.1f)
                // Going up → medium RPM
                targetRPM = mediumRPM;
            else if (input > -0.1f && input <= 0.1f)
                // Hover → idle RPM
                targetRPM = idleRPM;
            else
                // Going down → rotors effectively off (or low spin)
                targetRPM = 0f;

            // Smooth RPM
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime * rpmChangeSpeed);

            // Spin blade
            var degreesPerSecond = currentRPM / 60f * 360f;
            rotorBlade.transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}