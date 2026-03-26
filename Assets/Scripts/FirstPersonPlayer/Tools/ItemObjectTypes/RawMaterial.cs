using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "RawMaterial", menuName = "Scriptable Objects/Items/Raw Material")]
    public class RawMaterial : MyBaseItem
    {
        public enum MaterialType
        {
            Steel
        }

        public MaterialType materialType = MaterialType.Steel;
    }
}
