using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Overview.NPC
{
    [CreateAssetMenu(
        fileName = "CreatureDefinition", menuName = "Scriptable Objects/Character/CreatureDefinition", order = 1)]
    public class CreatureDefinition : ScriptableObject
    {
        public string creatureId;

        [FoldoutGroup("Animations")] public List<GestureEntry> gestures = new();

        public AnimationClip idleClip;

        public AnimationClip GetGesture(string key)
        {
            foreach (var g in gestures)
                if (g.key == key)
                    return g.clip;

            return null;
        }
    }
}
