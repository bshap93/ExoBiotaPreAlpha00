using System;
using Helpers.Events.Status;
using Manager;
using UnityEngine;

namespace Helpers.Collider
{
    public enum LiquidType
    {
        Water,
        HotspringPoolWater,
        DeathFog
    }

    public class LiquidEffectTrigger : MonoBehaviour
    {
        [SerializeField] LiquidType liquidType;
        void Start()
        {
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
                switch (liquidType)
                {
                    case LiquidType.HotspringPoolWater:
                        var isPlayerHealthMaxed = false;
                        var statsMgr = PlayerMutableStatsManager.Instance;
                        if (statsMgr != null)
                            if (statsMgr.CurrentHealth >= statsMgr.CurrentMaxHealth - 0.5f)
                                isPlayerHealthMaxed = true;

                        if (!isPlayerHealthMaxed)
                            PlayerStatsEvent.Trigger(
                                PlayerStatsEvent.PlayerStat.CurrentHealth,
                                PlayerStatsEvent.PlayerStatChangeType.Increase,
                                20f);

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
    }
}
