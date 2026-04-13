using CompassNavigatorPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace OWPData.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "CharacterStatProfile",
        menuName = "Scriptable Objects/Character/Character Stat Profile")]
    public class CharacterStatProfile : ScriptableObject
    {
        [FormerlySerializedAs("InitialMaxStamina")] [Header("Initial Stats")]
        public float InitialMaxHealth;

        public float InitialHealth;

        public Color initialUpgradeColor;

        [Header("Inventory Stats")] public int InitialWeightLimit;
        [FormerlySerializedAs("InitialPrimaryCurrency")]
        [FormerlySerializedAs("InitialCurrency")]
        [Header("Currency Stats")]
        public float initialPrimaryCurrency;
        [FormerlySerializedAs("InitialSecondaryCurrency")]
        public float initialSecondaryCurrency;


        [Header("Skip Tutorial")] public bool SkipTutorial;


        [FormerlySerializedAs("initialJetPackParticleMaterial")]
        public Color initialJetPackParticleColor;

        public float initialScannerRange;
        public ScanProfile initialScanProfile;
    }
}
