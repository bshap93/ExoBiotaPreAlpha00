using UnityEngine;

namespace Helpers.Collider
{
    public class VFXColliderTrigger : MonoBehaviour
    {
        public GameObject[] vfxToToggle;
        void Start()
        {
            DeactivateVFX();
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) ActivateVFX();
        }

        void OnTriggerExit(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer")) DeactivateVFX();
        }

        void ActivateVFX()
        {
            foreach (var vfx in vfxToToggle)
                if (vfx != null)
                    vfx.SetActive(true);
        }

        void DeactivateVFX()
        {
            foreach (var vfx in vfxToToggle)
                if (vfx != null)
                    vfx.SetActive(false);
        }
    }
}
