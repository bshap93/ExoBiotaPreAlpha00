using System.Collections;
using Domains.Gameplay.Equipment.Events;
using Helpers.Events;
using Helpers.Events.Combat;
using MoreMountains.Tools;
using UnityEngine;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace FirstPersonPlayer.UI.ProgressBars
{
    public class ProgressBarBlue : MonoBehaviour, MMEventListener<EquipmentEvent>,
        MMEventListener<BioSampleEvent>, MMEventListener<ScannerEvent>,
        MMEventListener<ChargeToolEvent>
    {
        [SerializeField] protected MMProgressBar cooldownProgressBar;
        [SerializeField] protected CanvasGroup cooldownCanvasGroup;

        Coroutine _running;

        void OnEnable()
        {
            this.MMEventStartListening<BioSampleEvent>();
            this.MMEventStartListening<EquipmentEvent>();
            this.MMEventStartListening<ScannerEvent>();
            this.MMEventStartListening<ChargeToolEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<BioSampleEvent>();
            this.MMEventStopListening<EquipmentEvent>();
            this.MMEventStopListening<ScannerEvent>();
            this.MMEventStopListening<ChargeToolEvent>();
        }

        public void OnMMEvent(BioSampleEvent e)
        {
            switch (e.EventType)
            {
                case BioSampleEventType.Abort:
                    StopAndHide();
                    break;

                case BioSampleEventType.StartCollection:
                    if (_running != null) StopCoroutine(_running);
                    _running = StartCoroutine(ShowCooldownBarCoroutine(e.Duration));
                    break;

                case BioSampleEventType.CompleteCollection:
                    if (_running != null)
                    {
                        StopCoroutine(_running);
                        _running = null;
                    }

                    ResetCooldownBar();
                    HideCooldownBar();
                    break;
            }
        }
        public void OnMMEvent(ChargeToolEvent eventType)
        {
            switch (eventType.EventType)
            {
                case ChargeToolEventType.Start:
                    if (cooldownProgressBar != null)
                    {
                        cooldownCanvasGroup.alpha = 1f;
                        cooldownProgressBar.UpdateBar01(eventType.FractionCharged);
                    }

                    break;
                case ChargeToolEventType.Update:
                    if (cooldownProgressBar != null)
                        // cooldownCanvasGroup.alpha = 1f;
                        cooldownProgressBar.UpdateBar01(eventType.FractionCharged);

                    break;
                case ChargeToolEventType.Release:
                    StopAndHide();
                    break;
                case ChargeToolEventType.Cancel:
                    StopAndHide();
                    break;
            }
        }

        public void OnMMEvent(EquipmentEvent eventType)
        {
            StopAndHide();
        }

        public void OnMMEvent(ScannerEvent eventType)
        {
            if (eventType.ScannerEventType == ScannerEventType.ExaminationStart)
            {
                if (_running != null) StopCoroutine(_running);
                _running = StartCoroutine(ShowCooldownBarCoroutine(eventType.Duration));
            }

            if (eventType.ScannerEventType == ScannerEventType.ExaminationEnd) StopAndHide();
        }

        void StopAndHide()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }

            ResetCooldownBar();
            HideCooldownBar();
        }


        // Catches Equipment Events and Disappears, Resets, or Equivalent
        public IEnumerator ShowCooldownBarCoroutine(float duration)
        {
            if (cooldownProgressBar == null) yield break;

            var elapsed = 0f;
            cooldownCanvasGroup.alpha = 1f;
            cooldownProgressBar.UpdateBar01(0f); // ← show full immediately


            while (elapsed < duration)
            {
                cooldownProgressBar.UpdateBar01(elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cooldownProgressBar.UpdateBar01(1f);
            cooldownCanvasGroup.alpha = 0f;
        }


        public void HideCooldownBar()
        {
            if (cooldownCanvasGroup) cooldownCanvasGroup.alpha = 0f;
        }

        public void ResetCooldownBar()
        {
            if (cooldownProgressBar) cooldownProgressBar.UpdateBar01(0f);
        }
    }
}
