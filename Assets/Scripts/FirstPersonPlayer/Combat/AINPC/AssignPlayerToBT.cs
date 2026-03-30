using FirstPersonPlayer.Interactable;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.Utilities;
using Manager.Global;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    [DisallowMultipleComponent]
    public class AssignPlayerToBT : MonoBehaviour
    {
        public Blackboard blackboard;
        public float delay = 0.5f;
        public string tgtTransformName = "playerTransform";
        public string tgtGameObjectName = "capsule";
        public string flagName;

        void Start()
        {
            Invoke(nameof(Assign), delay);
        }

        void Assign()
        {
            if (blackboard == null)
                blackboard = GetComponent<Blackboard>();


            // Get the top-level PlayerRoot
            var root = GameStateManager.Instance.PlayerRoot;
            if (root == null)
            {
                Debug.LogError("No PlayerRoot found!");
                return;
            }

            // Get the first active child (your actual moving pawn)
            Transform movingPawn = null;

            foreach (Transform child in root)
                if (child.gameObject.activeInHierarchy)
                {
                    movingPawn = child;
                    break;
                }

            if (movingPawn == null)
            {
                var player = FindFirstObjectByType<PlayerInteraction>();

                if (player == null)
                {
                    Debug.LogWarning("No moving player pawn found under PlayerRoot.");
                    return;
                }

                movingPawn = player.gameObject.transform;
            }

            var capsuleScaler = movingPawn.GetComponentInChildren<CharacterGraphicsScaler>();

            if (capsuleScaler == null)
            {
                Debug.LogError("No CharacterGraphicsScaler found on the moving player pawn.");
                return;
            }

            // Assign THIS instead of the root
            blackboard.SetVariableValue(tgtTransformName, movingPawn);
            blackboard.SetVariableValue(tgtGameObjectName, capsuleScaler.gameObject);

            if (!flagName.IsNullOrEmpty())
                blackboard.SetVariableValue(flagName, true);
        }
    }
}
