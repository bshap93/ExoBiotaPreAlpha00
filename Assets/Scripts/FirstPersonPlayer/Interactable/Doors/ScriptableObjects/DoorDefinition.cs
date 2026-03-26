using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.Doors.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DoorDefinition", menuName = "Scriptable Objects/Doors/DoorDefinition")]
    public class DoorDefinition : ScriptableObject
    {
        public enum AccessMode
        {
            RequireAny,
            RequireAll,
            Unlocked
        }

        public string doorId; // stable ID (GUID or hand-written)

        [Tooltip("All keys required if RequireAll, otherwise any one key is enough.")]
        public List<string> requiredKeyIds = new();

        public AccessMode accessMode = AccessMode.RequireAny;
    }
}