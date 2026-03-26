using System;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.ScriptableObjects.BioticAbility
{
    [Serializable]
    public class KeyValuesByExobioticLevel
    {
        [Range(1, 20)] public int ExobioticLevel = 1;
        public float ContaminationCost;
        public float AbilityRange;
    }

    [CreateAssetMenu(
        fileName = "PlayerBioticAbility_",
        menuName = "Scriptable Objects/Character/First Person Player/Player Biotic Ability",
        order = 0)]
    public class BioticAbility : ScriptableObject
    {
        [Serializable]
        public enum BioticAbilityType
        {
            RangedHitscanAttack,
            RangedSlowProjectileAttack,
            AreaOfEffectAttack,
            AreaOfEffectSpecial,
            RangedEffect,
            InstantiateObject,
            Passive
        }

        [Serializable]
        public enum SpecialEffectType
        {
            None,
            Placate
        }

        public enum UsageType
        {
            SingleUse,
            UseWhileHeld
        }

        [FormerlySerializedAs("specialType")] [ShowIf("abilityType", BioticAbilityType.AreaOfEffectSpecial)]
        public SpecialEffectType specialEffectType;

        [FormerlySerializedAs("ContaminationCostsByExobioticLevel")] [ShowIf("usageType", UsageType.SingleUse)]
        public KeyValuesByExobioticLevel[] contaminationCostsByExobioticLevel;

        [SerializeField] AudioClip injectionOfAbilityFluidClip;

        [Header("Basic Properties")] public string displayName;
        public BioticAbilityType abilityType;
        public UsageType usageType;


        [ShowIf("usageType", UsageType.UseWhileHeld)]
        public float contaminationCostPerSecond; // Cost while held

        [Header("Attack Effects")] [SerializeField]
        bool hasAttackEffect;
        [FormerlySerializedAs("attackEffect")] [ShowIf("hasAttackEffect")] [SerializeField]
        PlayerAttack playerAttack;


        // public float abilityBaseRange;
        public float bioticReductionFactor = 0.05f;

        [FormerlySerializedAs("cooldownTime")] public float baseCooldownTime = 1f; // Cooldown time in seconds

        public string UniqueID => name; // Using the asset's name as a unique identifier

        public float GetContaminationCostForExobioticLevel(int exobioticLevel)
        {
            foreach (var entry in contaminationCostsByExobioticLevel)
                if (entry.ExobioticLevel == exobioticLevel)
                    return entry.ContaminationCost;

            Debug.LogWarning($"No contamination cost found for exobiotic level {exobioticLevel}. Returning 0.");
            return 0f; // Default cost if not found
        }

        public float GetAbilityRangeForExobioticLevel(int exobioticLevel)
        {
            foreach (var entry in contaminationCostsByExobioticLevel)
                if (entry.ExobioticLevel == exobioticLevel)
                    return entry.AbilityRange;

            Debug.LogWarning($"No ability range found for exobiotic level {exobioticLevel}. Returning 0.");
            return 0f; // Default range if not found
        }

        public PlayerAttack GetPlayerAttack()
        {
            return hasAttackEffect ? playerAttack : null;
        }

        public SpecialEffectType GetSpecialType()
        {
            if (abilityType != BioticAbilityType.AreaOfEffectSpecial) // Add &&s when needed
                return SpecialEffectType.None;

            return specialEffectType;
        }

        // public GameObject runtimeAbilityPrefab;
    }
}
