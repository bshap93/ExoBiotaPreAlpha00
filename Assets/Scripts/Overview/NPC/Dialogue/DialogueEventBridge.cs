using System.Collections;
using Events;
using Helpers.Events.Dialog;
using Manager.DialogueScene;
using MoreMountains.Tools;
using Sirenix.Utilities;
using UnityEngine;

namespace Overview.NPC.Dialogue
{
    public class DialogueEventBridge : MonoBehaviour,
        MMEventListener<FirstPersonDialogueEvent>
    {
        [SerializeField] NpcDatabase npcDatabase;
        [SerializeField] DialogueManager dialogueManager;

        public void OnEnable()
        {
            this.MMEventStartListening();
        }

        public void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(FirstPersonDialogueEvent e)
        {
            if (e.Type != FirstPersonDialogueEventType.StartDialogue) return;

            NpcDefinition foundNpcDefinition = null;
            foreach (var npc in npcDatabase.npcDefinitions)
                if (e.NPCId == npc.npcId)
                {
                    foundNpcDefinition = npc;
                    break;
                }

            if (foundNpcDefinition == null)
                // if (!npcDatabase.TryGet(e.NPCId, out var def))
            {
                Debug.LogWarning($"No NPC with id {e.NPCId}");
                return;
            }

            if (!e.StartNodeOverride.IsNullOrWhitespace())
                dialogueManager.OpenNPCDialogue(
                    foundNpcDefinition, startNodeOverride: e.StartNodeOverride, autoClose: true);
            else
                dialogueManager.OpenNPCDialogue(foundNpcDefinition);
        }

        // public void OnMMEvent(OverviewLocationEvent e)
        // {
        //     if (e.LocationActionType != LocationActionType.Approach) return;
        //
        //     var locationDefinition = DockManager.Instance.GetLocationDefinition(e.LocationId);
        //
        //     if (locationDefinition == null)
        //     {
        //         Debug.LogWarning($"No location definition for {e.LocationId}");
        //         return;
        //     }
        //
        //     if (e.LocationType == LocationType.Dirigible)
        //
        //         if (locationDefinition.npcInResidenceId == "None")
        //         {
        //             Debug.LogWarning($"No npc in residence at {e.LocationId}");
        //             StartCoroutine(WaitAndThenRetreat(e));
        //             return;
        //         }
        //
        //     var startNode = e.StartNodeOverride;
        //
        //
        //     // LocationId IS the NPC id now
        //     if (!npcDatabase.TryGet(locationDefinition.npcInResidenceId, out var def))
        //     {
        //         Debug.LogWarning($"No NPC with id {locationDefinition.npcInResidenceId}");
        //         return;
        //     }
        //
        //
        //     dialogueManager.OpenNPCDialogue(
        //         def, null, true,
        //         string.IsNullOrEmpty(startNode) ? null : startNode);
        // }
        //

        IEnumerator WaitAndThenRetreat(OverviewLocationEvent e)
        {
            yield return new WaitForSeconds(0.1f);
            OverviewLocationEvent.Trigger(e.LocationType, LocationActionType.RetreatFrom, e.LocationId, null);
        }
    }
}
