using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyVFXSet",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy VFX Set",
        order = 0)]
    public class CreatureEffectsAndFeedbacks : ScriptableObject
    {
        public GameObject basicHitVFX;
        public GameObject heavyHitVFX;

        public GameObject basicDeathFeedbacks;
        public GameObject basicHitFeedbacks;
        public GameObject heavyHitFeedbacks;

        [Header("Feedbacks")] public GameObject idleEveryTenTofifteenSecondsFeedbacks;
        public GameObject secondaryIdlePeriodicFeedbacks;
        public GameObject idleWhenPlayerIsNearFeedbacks;
        public GameObject playerInteractsWithCreatureFeeedbacks;
        // Only if 
        public GameObject dialoguePunctutationFeedbacks;

        [FormerlySerializedAs("idleEveryTenToFifteenSeconds_MinTime")] [Header("Feeddbacks - Timings")]
        public float idleEveryTenToFifteenSecondsMinTime = 10f;
        [FormerlySerializedAs("IdleEveryTenToFifteenSecondsMaxTime")]
        public float idleEveryTenToFifteenSecondsMaxTime = 15f;

        [FormerlySerializedAs("secondaryIdlePeriodicFeedbacks_MinTime")]
        public float secondaryIdlePeriodicFeedbacksMinTime = 20f;
        [FormerlySerializedAs("secondaryIdlePeriodicFeedbacks_MaxTime")]
        public float secondaryIdlePeriodicFeedbacksMaxTime = 30f;

        [FormerlySerializedAs("idleWhenPlayerIsNearFeedbacks_Time")]
        public float idleWhenPlayerIsNearFeedbacksTime = 10f;
    }
}
