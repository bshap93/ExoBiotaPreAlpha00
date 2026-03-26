using UnityEngine;

namespace FirstPersonPlayer.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "ElevatorTypeInfo", menuName = "Scriptable Objects/Machines/ElevatorTypeInfo",
        order = 1)]
    public class ElevatorTypeInfo : ScriptableObject
    {
        public string elevatorName;

        public bool canInteract = true;
        public string elevatorID;

        public int numberOfFloors;

        public float travelTime;

        public int[] initiallyAccessibleFloors;

        public Sprite[] floorSprites;

        public Sprite defaultFloorSprite;
    }
}
