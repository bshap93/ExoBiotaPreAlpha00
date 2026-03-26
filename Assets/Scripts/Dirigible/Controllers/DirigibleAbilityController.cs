using Helpers.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dirigible.Controllers
{
    public class DirigibleAbilityController : MonoBehaviour
    {
        [FormerlySerializedAs("applyEffect")] public bool applyAbility;
        [FormerlySerializedAs("changeEffect")] public float changeAbility;
        public DirigibleEquipment.DirigibleEquipmentSlot activeAbilitySlot;

        [SerializeField] MMFeedbacks ligntsOnFeedback;
        [SerializeField] MMFeedbacks lightsOffFeedback;

        [SerializeField] Light[] lightsToToggle;
        [SerializeField] Material[] materialsToToggle;
        bool _lightsOn;
        public void ToggleLights()
        {
            LightEvent.Trigger(_lightsOn ? LightEventType.TurnOff : LightEventType.TurnOn);
            _lightsOn = !_lightsOn;
            if (_lightsOn)
                ligntsOnFeedback?.PlayFeedbacks();
            else
                lightsOffFeedback?.PlayFeedbacks();

            foreach (var light in lightsToToggle)
                if (light != null)
                    light.enabled = _lightsOn;

            foreach (var mat in materialsToToggle)
                if (mat != null)
                {
                    if (_lightsOn)
                        mat.EnableKeyword("_EMISSION");
                    else
                        mat.DisableKeyword("_EMISSION");
                }
        }
    }
}
