using System;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(
        fileName = "New BioOrganismType", menuName = "Scriptable Objects/Items/BioOrganismType",
        order = 1)]
    public class BioOrganismType : ScriptableObject
    {
        [FormerlySerializedAs("OrganismID")] public string organismID;
        [FormerlySerializedAs("OrganismName")] public string organismName;
        public Sprite organismIcon;
        public string shortDescription;
        public string fullDescription;

        public IdentificationMode identificationMode = IdentificationMode.RecognizableOnSight;


        // Optional: a redacted name/icon for unknowns
        public string UnknownName = "Unknown Item";

        public string UnknownDescription;

        public Sprite ActionIcon;

        public bool Eviscerable; // can be eviscerated for parts (e.g. multi-cellular organisms)
    }
}
