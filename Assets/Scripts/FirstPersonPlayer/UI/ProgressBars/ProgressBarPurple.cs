using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.UI.ProgressBars
{
    public class ProgressBarPurple : MonoBehaviour
    {
        [SerializeField] protected MMProgressBar cooldownProgressBar;
        [SerializeField] protected CanvasGroup cooldownCanvasGroup;

        protected Coroutine Running;

        protected virtual void StopAndHide()
        {
            if (Running != null)
            {
                StopCoroutine(Running);
                Running = null;
            }

            ResetCooldownBar();
            HideCooldownBar();
        }

        protected virtual void HideCooldownBar()
        {
            if (cooldownCanvasGroup) cooldownCanvasGroup.alpha = 0f;
        }

        protected virtual void ResetCooldownBar()
        {
            if (cooldownProgressBar) cooldownProgressBar.UpdateBar01(0f);
        }
    }
}
