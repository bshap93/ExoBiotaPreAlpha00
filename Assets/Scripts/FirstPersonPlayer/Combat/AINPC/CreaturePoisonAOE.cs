using System.Collections;
using Helpers.Events.Combat;
using Manager.Status;
using Manager.Status.Scriptable;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class CreaturePoisonAOE : MonoBehaviour
    {
        [SerializeField] GameObject poisonAOEEffect;
        [SerializeField] float effectDuration = 5f;
        [SerializeField] ParticleSystem[] poisonEffectParticles;

        [Header("Status Effect To Apply")]
        [Tooltip("Must match the effectID on your Poison StatusEffect ScriptableObject")]
        [SerializeField]
        string poisonEffectID = "Poison";
        [SerializeField] string catalogID = "";
        bool _hasAppliedToPlayer;

        bool _isActivelyPoisoning;

        void OnTriggerEnter(Collider other)
        {
        }

        void OnTriggerStay(Collider other)
        {
            if (!_isActivelyPoisoning) return;
            if (_hasAppliedToPlayer) return;
            if (!other.CompareTag("FirstPersonPlayer")) return;

            // Don't stack — skip if player already has this poison
            if (PlayerStatusEffectManager.Instance.HasEffect(poisonEffectID)) return;

            _hasAppliedToPlayer = true;

            PlayerStatusEffectEvent.Trigger(
                PlayerStatusEffectEvent.StatusEffectEventType.Apply,
                poisonEffectID,
                catalogID,
                PlayerStatusEffectEvent.DirectionOfEvent.Inbound,
                StatusEffect.StatusEffectKind.None
            );
        }

        public void ReleasePoison()
        {
            if (_isActivelyPoisoning) return;
            _isActivelyPoisoning = true;
            _hasAppliedToPlayer = false;
            poisonAOEEffect.SetActive(true);
            foreach (var particle in poisonEffectParticles) particle.Play();
            StartCoroutine(Cleanup());
        }

        IEnumerator Cleanup()
        {
            yield return new WaitForSeconds(effectDuration);
            _isActivelyPoisoning = false;
            foreach (var particle in poisonEffectParticles) particle.Stop();
            poisonAOEEffect.SetActive(false);
        }
    }
}
