using UnityEngine;

namespace FirstPersonPlayer.Interactable.Vat.ScriptableObj
{
    [CreateAssetMenu(fileName = "ReagenLiquidObject", menuName = "Scriptable Objects/Liquids/ReagentLiquid", order = 1)]
    public class ReagentLiquidObject : ScriptableObject
    {
        public string reagentId;
    }
}