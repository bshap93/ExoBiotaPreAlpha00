using Helpers.Events.Spawn;
using Helpers.Events.Status;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.FeedbackControllers
{
    public class ContaminationFeedbackController : MonoBehaviour, MMEventListener<PlayerStatsEvent> //   ,
    // MMEventListener<ContaminationCUEvent>
    {
        public MMF_Player onSpike; // optional: play on big burst
        [SerializeField] MMFeedbacks contaminationIncreaseFeedbacks;

        [Header("Tuning")] public float maxContamination = 20f; // match your PlayerStatsManager max
        public float spikeThreshold = 5f; // amount per second to trigger spike feedback
        public float spikeCooldown = 2f; // seconds between spikes

        [FormerlySerializedAs("playerStatsManager")] [SerializeField]
        PlayerMutableStatsManager playerMutableStatsManager;

        float amountCntmLastSecond;
        float timeSinceLastSpike;

        void OnEnable()
        {
            this.MMEventStartListening();
            // this.MMEventStartListening<ContaminationCUEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
            // this.MMEventStopListening<ContaminationCUEvent>();
        }


        public void OnMMEvent(PlayerStatsEvent eventType)
        {
            if (eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentContamination &&
                eventType.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
            {
                if (playerMutableStatsManager.CurrentContamination >= maxContamination)
                    return; // no feedback at max contamination

                var incBy = eventType.Amount;
                var overTime = eventType.OverTime;

                amountCntmLastSecond += incBy;
                timeSinceLastSpike += Time.deltaTime;

                if (timeSinceLastSpike < spikeCooldown) return; // still cooling down
                if (amountCntmLastSecond < spikeThreshold) return; // not enough increase 


                onSpike.PlayFeedbacks();

                ContaminationSpikeEvent.Trigger(playerMutableStatsManager.CurrentContamination);

                amountCntmLastSecond = 0f;
                timeSinceLastSpike = 0f; // reset cooldown
            }
        }
    }
}
