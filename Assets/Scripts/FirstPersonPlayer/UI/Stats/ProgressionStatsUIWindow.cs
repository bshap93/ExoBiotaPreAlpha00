using System;
using Helpers.Events;
using Helpers.Events.Progression;
using Manager.ProgressionMangers;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace FirstPersonPlayer.UI.Stats
{
    public class ProgressionStatsUIWindow : MonoBehaviour, MMEventListener<LoadedManagerEvent>,
        MMEventListener<ProgressionUpdateListenerNotifier>
    {
        [Header("Progression")] [SerializeField]
        LevelingManager levelingManager;
        [SerializeField] TMP_Text xpAmtText;
        [SerializeField] TMP_Text levelIntText;
        void Start()
        {
            Initialize();
        }
        void OnEnable()
        {
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<ProgressionUpdateListenerNotifier>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<ProgressionUpdateListenerNotifier>();
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            // throw new NotImplementedException();
        }
        public void OnMMEvent(ProgressionUpdateListenerNotifier eventType)
        {
            xpAmtText.text = eventType.CurrentTotalXP + "/" + levelingManager.TotalXpNeededForNextLevel;
            levelIntText.text = eventType.CurrentLevel.ToString();
        }
        void Initialize()
        {
            xpAmtText.text = levelingManager.CurrentTotalXP + "/" + levelingManager.TotalXpNeededForNextLevel;
            levelIntText.text = levelingManager.CurrentLevel.ToString();
        }
    }
}
