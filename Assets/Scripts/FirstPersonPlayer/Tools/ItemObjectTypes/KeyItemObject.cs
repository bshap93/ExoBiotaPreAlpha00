using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    public enum KeyItemType
    {
        None
    }

    [CreateAssetMenu(
        fileName = "KeyItemObject",
        menuName = "Scriptable Objects/Items/KeyItemObject",
        order = 0)]
    public class KeyItemObject : MyBaseItem
    {
        public KeyItemType keyItemType = KeyItemType.None;
        public string KeyID => ItemID;
    }
}
