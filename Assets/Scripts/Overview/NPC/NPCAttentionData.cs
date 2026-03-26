using System;

namespace Overview.NPC
{
    [Flags]
    public enum AttentionReason
    {
        None = 0,
        NewDialogue = 1 << 0, // unseen lines / Yarn var says so
        ObjectiveOffer = 1 << 1, // prerequisite met, not yet added
        ObjectiveTurnIn = 1 << 2 // objectives complete for this NPC
    }

    [Serializable]
    public struct NPCAttentionData
    {
        public string npcId;
        public AttentionReason reasons;
        public int version; // increment whenever reasons change
        public DateTime lastChangedUtc;
    }
}