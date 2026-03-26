using FirstPersonPlayer.Tools.ItemObjectTypes;
using UnityEngine;

namespace FirstPersonPlayer.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "MachineType", menuName = "Scriptable Objects/Machines/MachineType",
        order = 1)]
    public class MachineType : ScriptableObject
    {
        public Sprite machineIcon;
        public string machineName;
        public string shortDescription;

        public float electricalOutput;

        public bool canInteract = true;
        public string machineID;

        public MyBaseItem powerSourceItem;
    }
}
