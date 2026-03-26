using System;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "CreatureSampleObject",
        menuName = "Scriptable Objects/Items/CreatureSampleObject",
        order = 0)]
    [Serializable]
    public class CreatureSampleObject : MyBaseItem
    {
        [FormerlySerializedAs("CreatureSourceType")]
        public CreatureType creatureSourceType;
        public BioOrganismType bioOrganismType;

        public bool hasAssociatedBioticAbility;

        [ShowIf("hasAssociatedBioticAbility")] public BioticAbilityToolWrapper associatedBioticAbility;

        public override bool Pick(string playerID)
        {
            var uniqueID = Guid.NewGuid().ToString();


            BioSampleEvent.Trigger(uniqueID, BioSampleEventType.CompleteCollection, bioOrganismType, 0f);
            return base.Pick(playerID);
        }
    }
}
