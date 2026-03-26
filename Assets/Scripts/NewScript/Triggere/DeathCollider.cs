using System;
using Helpers.Events;
using Manager.FirstPerson;
using UnityEngine;
using Utilities.Interface;

namespace NewScript.Triggere
{
    public class DeathCollider : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;
        [SerializeField] DeathInformation deathInformation;
        // [SerializeField] bool arrestCameraOnDeath = true;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
                PlayerDeathEvent.Trigger(deathInformation);
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
    }
}
