using DG.Tweening;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using Manager.Settings;
using MoreMountains.Tools;
using Rewired.Integration.Cinemachine3;
using Unity.Cinemachine;
using UnityEngine;

namespace Helpers
{
    public class FPCameraEventHandler : MonoBehaviour, MMEventListener<PlayerDamageEvent>,
        MMEventListener<PlayerDeathEvent>, MMEventListener<NPCAttackEvent>,
        MMEventListener<DialogueCameraEvent>, MMEventListener<GlobalSettingsEvent>
    {
        [SerializeField] CinemachineCamera cinemachineCamera;
        // [SerializeField] DOTweenAnimation dOTweenAnimation;
        [SerializeField] RewiredCinemachineInputAxisController axisController;

        [SerializeField] float defaultFOV;

        void Start()
        {
            var gsm = GlobalSettingsManager.Instance;
            if (gsm != null)
                cinemachineCamera.Lens.FieldOfView = gsm.FieldOfView;
            else
                cinemachineCamera.Lens.FieldOfView = defaultFOV;
        }

        void OnEnable()
        {
            this.MMEventStartListening<PlayerDamageEvent>();
            this.MMEventStartListening<PlayerDeathEvent>();
            this.MMEventStartListening<NPCAttackEvent>();
            this.MMEventStartListening<DialogueCameraEvent>();
            this.MMEventStartListening<GlobalSettingsEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<PlayerDamageEvent>();
            this.MMEventStopListening<PlayerDeathEvent>();
            this.MMEventStopListening<NPCAttackEvent>();
            this.MMEventStopListening<DialogueCameraEvent>();
            this.MMEventStopListening<GlobalSettingsEvent>();
        }
        public void OnMMEvent(DialogueCameraEvent e)
        {
            if (axisController == null) return;

            axisController.enabled = e.Type != DialogueCameraEventType.FocusOnTarget;
        }
        public void OnMMEvent(GlobalSettingsEvent eventType)
        {
            if (eventType.EventType == GlobalSettingsEventType.FieldOfViewChanged)
            {
                cinemachineCamera.Lens.FieldOfView = eventType.FloatValue;
                Debug.Log("FieldOfView: " + cinemachineCamera.Lens.FieldOfView);
            }
        }
        public void OnMMEvent(NPCAttackEvent eventType)
        {
            if (eventType.Attack.rawDamage > 0)
            {
                // Shake camera based on attack damage. Higher damage = more shake
                var intensity = Mathf.Clamp(eventType.Attack.rawDamage / 100f, 0.05f, 0.2f);
                ShakeCamera(intensity, 0.2f);
            }
        }

        public void OnMMEvent(PlayerDamageEvent e)
        {
            if (e.HitType == PlayerDamageEvent.HitTypes.CriticalHit)
                ShakeCamera(0.1f, 0.05f);
            else if (e.HitType == PlayerDamageEvent.HitTypes.Normal) ShakeCamera(0.05f, 0.05f);
        }
        public void OnMMEvent(PlayerDeathEvent eventType)
        {
            axisController.enabled = false;
        }

        void ShakeCamera(float intensity, float duration)
        {
            transform.DOShakePosition(duration, new Vector3(intensity, intensity, intensity))
                .SetEase(Ease.InOutElastic).SetLoops(4, LoopType.Yoyo);
        }
    }
}
