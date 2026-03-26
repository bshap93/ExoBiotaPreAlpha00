using UnityEngine;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	
// Not different from the original code, but added using directives for clarity.

namespace FirstPersonPlayer.UI
{
    [CreateAssetMenu(fileName = "ReticleState", menuName = "UI/Reticle State")]
    public class ReticleState : ScriptableObject
    {
        [Header("Visual Settings")] public Sprite reticleSprite;

        public Color reticleColor = Color.white;

        [Header("State Information")] public string stateName;

        [TextArea(2, 4)] public string stateDescription;
    }
}