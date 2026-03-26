using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Tools;
using Helpers.Events.NPCs;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class BlockFBHelper : MonoBehaviour, MMEventListener<NPCAttackEvent>
{
    [SerializeField] MMFeedbacks basicBlockMeleeFeedbacks;
    [SerializeField] MMFeedbacks basicBlockRangedFeedbacks;
    PlayerEquipment _playerEquipment;

    void Start()
    {
        _playerEquipment = PlayerEquipment.InstanceRight;
    }
    void OnEnable()
    {
        this.MMEventStartListening();
    }
    void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(NPCAttackEvent eventType)
    {
        if (_playerEquipment.IsBlocking)
        {
            if (eventType.Attack != null && eventType.Attack.attackType == NPCAttackType.Melee)
            {
                Debug.Log("Playing block melee feedbacks");
                basicBlockMeleeFeedbacks?.PlayFeedbacks();
            }
            else if (eventType.Attack != null && eventType.Attack.attackType == NPCAttackType.Ranged)
            {
                basicBlockRangedFeedbacks?.PlayFeedbacks();
            }
        }
    }
}
