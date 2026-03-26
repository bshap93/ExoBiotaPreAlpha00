using UnityEngine;
using UnityEngine.Serialization;

namespace OWPData.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "PlayerStatsSheet", menuName = "Scriptable Objects/Character/PlayerStatsSheet", order = 0)]
    public class PlayerStatsSheet : ScriptableObject
    {
        [Header("Current Stats")] public float currentHealth;
        public float currentStamina;
        public float currentVision;
        public float currentContamination;

        [Header("Base Max Stats")] public float baseMaxHealth;
        public float baseMaxStamina;
        public float baseMaxVision;
        public float baseMaxContamination;

        [Header("Contamination Specific")] public float contaminationPointsPerCU;

        [Header("Current Max Stats")] public float currentMaxHealth;
        // public float currentMaxStamina;
        public float currentMaxVision;
        public float currentMaxContamination;

        // public float sprintStaminaDrainPerSecond;
        [FormerlySerializedAs("baseStaminaRestorePerSecond")]
        public float initialBaseStaminaRestoreRate;
        public float staminaReductionReducePerPoint;
        public float baseContaminationDecreasePerSecond;


        public bool isPlayerExoBiote;
    }
}
