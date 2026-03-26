using System;
using System.Collections.Generic;
using Helpers.Events.Dialog;
using Manager.DialogueScene;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace Overview.NPC
{
    [Serializable]
    public struct StartNodeEntry
    {
        public string locationId; // e.g. "Foreman_MineCamp"
        public string node; // e.g. "foreman_mine_start"
    }

    [Serializable]
    public struct GestureEntry
    {
        public string key; // e.g. "shrug", "scoff", "wave"
        public AnimationClip clip; // NPC-specific animation
    }

    [Serializable]
    public struct IdleLoopingEntry
    {
        public string key; // e.g. "shrug", "scoff", "wave"
        public AnimationClip clip; // NPC-specific animation
    }


    [Serializable]
    public struct SoundEntry
    {
        public string key; // e.g. "greeting", "farewell"
        public AudioClip clip; // NPC-specific sound
    }

    [CreateAssetMenu(fileName = "NpcDefinition", menuName = "Scriptable Objects/Character/NpcDefinition", order = 1)]
    public class NpcDefinition : ScriptableObject
    {
        public string characterName;
        [ValueDropdown("GetNpcIdOptions")] public string npcId; // also the locationId

        public string startNode; // ONE node only

        [ValueDropdown("GetLocationIdOptions")]
        public string locationId;

        public YarnProject yarnProject;
        public GameObject characterPrefab;

        [FoldoutGroup("Animations")] public List<GestureEntry> gestures = new();
        [FoldoutGroup("Idle Animations")] public List<IdleLoopingEntry> idleLoopingAnimations = new();
        public int initialIdleLoopingAnimationIndex;

        [FoldoutGroup("Sounds")] public List<SoundEntry> sounds = new();


        [Title("Available Start Nodes")] [InfoBox("List all dialogue entry points for this NPC")]
        public string[] availableStartNodes; // all possible start nodes

        [FormerlySerializedAs("NativeLanguage")]
        public LanguageType nativeLanguage;

        public bool hasAvatarDiorama = true;

        public float gestureTransitionDuration = 0.2f;
        public float idleTransitionDuration = 0.2f;
        [Header("Additional Optional Info")] public Sprite characterIcon;
        public string npcDescription;

        public bool givesXpForFirstMeeting;
        [ShowIf("givesXpForFirstMeeting")] public int xpForFirstMeeting;


        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        static string[] GetLocationIdOptions()
        {
            // return DockManager.GetLocationIdOptions();
            // return DialogueManager.GetAllLocationIdOptions();
            return new[] { "Location1", "Location2", "Location3" };
        }

        public AnimationClip GetGesture(string key)
        {
            foreach (var g in gestures)
                if (g.key == key)
                    return g.clip;

            return null;
        }
        public AnimationClip GetIdleLoopingClip(string key)
        {
            foreach (var g in idleLoopingAnimations)
                if (g.key == key)
                    return g.clip;

            return null;
        }
        public AudioClip GetDialogueSound(string key)
        {
            foreach (var s in sounds)
                if (s.key == key)
                    return s.clip;

            return null;
        }
        public AnimationClip GetDefaultIdleAnimation()
        {
            if (idleLoopingAnimations.Count > initialIdleLoopingAnimationIndex)
                return idleLoopingAnimations[initialIdleLoopingAnimationIndex].clip;

            return null;
        }
    }
}
