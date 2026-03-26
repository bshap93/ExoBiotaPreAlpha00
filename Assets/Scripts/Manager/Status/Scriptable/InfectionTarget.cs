using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager.Status.Scriptable
{
    public enum InfectionSite
    {
        Skin01,
        Eyes01,
        Lungs01,
        Heart01,
        Brain01
    }

    [Serializable]
    public class InfectionObject
    {
        public string infectionName;
        public string infectionSiteID;
        public InfectionSite site;
        [Range(0f, 1f)] public float baseProbability;

        [ReadOnly] public float currentProbability;

        public StatusEffect statusEffectOfInfection;
        public bool canBeRemovedViaDecontamination;
    }
}
