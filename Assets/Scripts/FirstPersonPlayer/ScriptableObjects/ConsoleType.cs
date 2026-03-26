using MoreMountains.InventoryEngine;
using UnityEngine;

namespace FirstPersonPlayer.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New ConsoleType", menuName = "Scriptable Objects/Items/ConsoleType",
        order = 1)]
    public class ConsoleType : ScriptableObject
    {
        public Sprite consoleIcon;
        public string consoleName;
        public string shortDescription;
        public Sprite actionIcon;
        public string actionText;

        public IdentificationMode identificationMode = IdentificationMode.RecognizableOnSight;

        public string UnknownName = "Unknown Console";

        public bool canInteract = true;
        public string consoleID;
    }
}