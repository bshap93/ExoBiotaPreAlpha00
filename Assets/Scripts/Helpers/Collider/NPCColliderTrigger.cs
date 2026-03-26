using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.Collider
{
    public class NPCColliderTrigger : MonoBehaviour
    {
        [FormerlySerializedAs("vfxToToggle")] public GameObject[] npcToToggle;
        void Start()
        {
            DeactivateNPC();
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) ActivateNPC();
        }

        void OnTriggerExit(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) DeactivateNPC();
        }

        void ActivateNPC()
        {
            foreach (var npc in npcToToggle)
                if (npc != null)
                    npc.SetActive(true);
        }

        void DeactivateNPC()
        {
            foreach (var npc in npcToToggle)
                if (npc != null)
                    npc.SetActive(false);
        }
    }
}
