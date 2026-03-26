using Dirigible.Controllers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Dirigible
{
    public class RotorAudioCtrl : MonoBehaviour
    {
        [Header("Quad Rotor Feedbacks")]
        // Unlike for rear prop, idle means spinning at low RPM, not stopped
        [SerializeField]
        private MMFeedbacks quadRotorFeedbacksIdleFB;

        [SerializeField] private MMFeedbacks quadRotorFeedbacksMediumRPMFB;
        [SerializeField] private MMFeedbacks quadRotorFeedbacksToOffFB;

        [Header("Rear Propeller Feedbacks")] [SerializeField]
        private MMFeedbacks revUpToLowRPMFeedbacks;

        [SerializeField] private MMFeedbacks revUpToMediumRPMFeedbacks;
        [SerializeField] private MMFeedbacks sustainLowRPMFeedbacks;
        [SerializeField] private MMFeedbacks sustainMediumRPMFeedbacks;
        [SerializeField] private MMFeedbacks revDownToLowRPMFeedbacks;
        [SerializeField] private MMFeedbacks revDownToIdleFeedbacks;

        public void UpdateAudio(DirigibleStatus status)
        {
        }
    }
}