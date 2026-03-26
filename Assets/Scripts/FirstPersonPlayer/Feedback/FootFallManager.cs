using FirstPersonPlayer.Interactable;
using Lightbug.CharacterControllerPro.Core;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Feedback
{
    internal class FootFallManager : MonoBehaviour
    {
        [Header("Footsteps")] [SerializeField] MMFeedbacks terrainFootstepFeedbacks;
        [SerializeField] MMFeedbacks rockFootstepFeedbacks;

        [SerializeField] MMFeedbacks defaultFootstepFeedbacks;
        [SerializeField] MMFeedbacks sandFootstepFeedbacks;
        [SerializeField] MMFeedbacks waterFootstepFeedbacks;
        [SerializeField] MMFeedbacks woodFootstepFeedbacks;
        [SerializeField] float baseStepInterval = 1.5f;

        [SerializeField] PlayerInteraction playerInteraction;

        [SerializeField] CharacterActor characterActor;

        float _footstepInterval;

        float _footstepTimer;

        float _secondTimer;

        bool _wasMovingLastFrame;


        void Awake()
        {
            if (characterActor == null)
                characterActor = FindFirstObjectByType<CharacterActor>();

            if (playerInteraction == null)
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        void Update()
        {
            UpdateFootsteps(Time.deltaTime);
        }

        void UpdateFootsteps(float dt)
        {
            var isMoving = characterActor.IsGrounded && characterActor.PlanarVelocity.magnitude > 0.01f;


            if (isMoving)
            {
                // Dynamically scale interval
                var speed = characterActor.PlanarVelocity.magnitude;
                _footstepInterval = baseStepInterval / Mathf.Max(speed, 0.1f);

                // Trigger first footstep as soon as movement begins
                if (!_wasMovingLastFrame)
                    _footstepTimer = _footstepInterval;

                // Run step timer
                if (_footstepTimer >= _footstepInterval)
                {
                    PlayFootfallFeedback();
                    _footstepTimer = 0f;
                }
                else
                    // ERROR - no dt in context
                {
                    _footstepTimer += dt;
                }
            }
            else
            {
                _footstepTimer = 0f;
            }

            _wasMovingLastFrame = isMoving;
        }


        void PlayFootfallFeedback()
        {
            // var textureIndex = playerInteraction?.GetGroundTextureIndex() ?? -1;
            var groundInfo = playerInteraction?.GetGroundInfo();
            if (groundInfo == null)
            {
                defaultFootstepFeedbacks?.PlayFeedbacks();
                return;
            }


            if (groundInfo.tag == "Water")
                waterFootstepFeedbacks?.PlayFeedbacks();
            else if (groundInfo.tag == "WoodSurface")
                woodFootstepFeedbacks?.PlayFeedbacks();
            else
                defaultFootstepFeedbacks?.PlayFeedbacks();


            defaultFootstepFeedbacks?.PlayFeedbacks();
        }
    }
}
