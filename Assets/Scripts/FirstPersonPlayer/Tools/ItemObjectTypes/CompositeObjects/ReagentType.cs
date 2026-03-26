using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects
{
    public enum ReagentClass
    {
        Catalyst,
        Solvent
    }

    [Serializable]
    [CreateAssetMenu(fileName = "ReagentType", menuName = "Scriptable Objects/CompositeTypes/ReagentType", order = 4)]
    public class ReagentType : LiquidType
    {
        [FormerlySerializedAs("ReagentClass")] public ReagentClass reagentClass;
        [FormerlySerializedAs("coreGradesDissolve")] [FormerlySerializedAs("CoreGrades")]
        public List<OuterCoreItemObject.CoreReactivity> coreGradesAffected;
    }
}
