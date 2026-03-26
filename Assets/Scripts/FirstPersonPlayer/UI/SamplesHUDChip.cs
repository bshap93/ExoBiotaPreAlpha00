// SamplesHudChip.cs

using System;
using Helpers.Events;
using Manager;
using Manager.Global;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstPersonPlayer.UI
{
    public class SamplesHudChip : MonoBehaviour, MMEventListener<BioSampleEvent>
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text countText;

        private void Start()
        {
            icon.sprite = ExaminationManager.Instance.iconRepository.sampleCartridgeIcon;

            Refresh();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
            Refresh();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(BioSampleEvent e)
        {
            switch (e.EventType)
            {
                case BioSampleEventType.CompleteCollection:
                    Refresh();
                    break;
                case BioSampleEventType.StartCollection:
                case BioSampleEventType.StartSequencing:
                case BioSampleEventType.CompletedSequencing:
                    break;
                case BioSampleEventType.RefreshUI:
                    Refresh();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Refresh()
        {
            var mgr = BioSamplesManager.Instance;
            if (mgr == null) return;
            var samples = mgr.GetSamplesCarried();
            countText.text = samples.Count.ToString();
        }
    }
}