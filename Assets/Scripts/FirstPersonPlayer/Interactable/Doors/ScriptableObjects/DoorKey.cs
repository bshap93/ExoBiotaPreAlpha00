using UnityEngine;

namespace FirstPersonPlayer.Interactable.Doors.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DoorKey", menuName = "Scriptable Objects/Doors/DoorKey")]
    public class DoorKey : ScriptableObject
    {
        public string keyId;
        public string displayName;
    }
}