using Helpers.Events.Status;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace NewScript.Triggere
{
    public class ContaminationZone : MonoBehaviour
    {
        [SerializeField] float contaminationPerSecond = 1f;
        [SerializeField] bool startsActive;
        // [SerializeField] float delayBeforeActive = 0f;
        [SerializeField] float activeDuration = Mathf.Infinity;
        // [SerializeField] bool delayBeforeActive;
        // [ShowIf("delayBeforeActive")] [SerializeField]
        // float delayDuration = 0.5f;
        [SerializeField] MMFeedbacks enterFeedbacks;
        [SerializeField] MMFeedbacks exitFeedbacks;
        [SerializeField] MMFeedbacks activationFeedbacks;
        [SerializeField] MMFeedbacks deactivationFeedbacks;
        [SerializeField] GameObject parentObject;

        bool _isActive;
        bool _isPlayerInside;

        float _timer;
        void Start()
        {
            _isActive = startsActive;
            if (_isActive)
            {
                _timer = 0f;
                activationFeedbacks?.PlayFeedbacks();
            }
        }

        void Update()
        {
            if (_isActive)
            {
                _timer += Time.deltaTime;
                if (_timer >= activeDuration)
                {
                    _isActive = false;
                    deactivationFeedbacks?.PlayFeedbacks();
                }
                // Optionally, you could disable the collider or visual effects here
            }

            if (_isActive && _isPlayerInside)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    contaminationPerSecond * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
            {
                _isPlayerInside = true;
                enterFeedbacks?.PlayFeedbacks();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!_isActive) return;
            if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
            {
                _isPlayerInside = false;
                exitFeedbacks?.PlayFeedbacks();
            }
        }
    }
}
