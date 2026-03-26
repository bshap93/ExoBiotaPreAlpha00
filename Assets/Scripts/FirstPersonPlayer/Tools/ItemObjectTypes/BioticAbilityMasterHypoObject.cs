using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "BioticAbilityMasterHypo", menuName = "Scriptable Objects/Items/BioticAbilityMasterHypo", order = 0)]
    public class BioticAbilityMasterHypoObject : BaseTool
    {
        [SerializeField] BioticAbilityToolWrapper bioticAbilityToolWrapper;
    }
}
