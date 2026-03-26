using UnityEngine;
using UnityEngine.Serialization;

namespace Dirigible
{
    public class FluorescentSpotlight : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] private Light spotlight; // Reference to the Light component

        [FormerlySerializedAs("intensity")] [SerializeField]
        private float defaultIntensity = 0.97f; // Intensity of the spotlight.

        [SerializeField] private Renderer spotlightRenderer; // Renderer for the spotlight mesh.

        private Material _spotlightObjectMaterial; // Material of the spotlight mesh.

        private void Start()
        {
            if (spotlightRenderer == null) return;

            _spotlightObjectMaterial = spotlightRenderer.material;
            ToggleLight(false); // Ensure the light is off at start
        }

        public void SetIntensity(float newIntensity)
        {
            if (spotlight != null)
            {
                defaultIntensity = newIntensity;
                spotlight.intensity = defaultIntensity;
            }
            else
            {
                Debug.LogWarning("FluorescentSpotlight: No Light component found.");
            }
        }

        public void ToggleLight(bool isOn)
        {
            if (spotlight != null)
            {
                spotlight.enabled = isOn;
                if (isOn)
                {
                    if (_spotlightObjectMaterial == null)
                    {
                        Debug.LogWarning("FluorescentSpotlight: No material assigned to spotlightRenderer.");
                        return;
                    }

                    _spotlightObjectMaterial.EnableKeyword("_EMISSION");
                    _spotlightObjectMaterial.SetColor(EmissionColor, Color.white * defaultIntensity);
                }
                else
                {
                    _spotlightObjectMaterial.DisableKeyword("_EMISSION");
                    _spotlightObjectMaterial.SetColor(EmissionColor, Color.black);
                }
            }
            else
            {
                Debug.LogWarning("FluorescentSpotlight: No Light component found.");
            }
        }
    }
}