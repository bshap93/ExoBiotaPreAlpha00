using Helpers.AnimancerHelper;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.CamperCrab
{
    public class CamperCrabAnimationHelperTester : MonoBehaviour
    {
        [SerializeField] private CamperCrabAnimationHelper helper;
        [SerializeField] private KeyCode idleKey = KeyCode.I;
        [SerializeField] private KeyCode turnLeftKey = KeyCode.Q;
        [SerializeField] private KeyCode turnRightKey = KeyCode.E;
        [SerializeField] private KeyCode attackKey = KeyCode.Space;

        private void Reset()
        {
            if (!helper) helper = GetComponentInChildren<CamperCrabAnimationHelper>();
        }

        private void Update()
        {
            if (!helper) return;
            if (Input.GetKeyDown(idleKey)) helper.PlayIdle();
            if (Input.GetKeyDown(turnLeftKey)) helper.PlayTurn(false);
            if (Input.GetKeyDown(turnRightKey)) helper.PlayTurn(true);
            if (Input.GetKeyDown(attackKey)) helper.TryAttack();
        }

        private void OnGUI()
        {
            const float w = 140, h = 28, pad = 8;
            float x = 12, y = 12;

            if (GUI.Button(new Rect(x, y, w, h), "Idle  (I)")) helper?.PlayIdle();
            y += h + pad;
            if (GUI.Button(new Rect(x, y, w, h), "Turn L (Q)")) helper?.PlayTurn(false);
            y += h + pad;
            if (GUI.Button(new Rect(x, y, w, h), "Turn R (E)")) helper?.PlayTurn(true);
            y += h + pad;

            // Disable the Attack button while on cooldown:
            var prev = GUI.enabled;
            GUI.enabled = !(helper?.IsOnAttackCooldown ?? false);
            if (GUI.Button(new Rect(x, y, w, h), "Attack (Space)")) helper?.TryAttack();
            GUI.enabled = prev;
        }
    }
}