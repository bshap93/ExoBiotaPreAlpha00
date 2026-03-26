using System.Collections;
using System.Collections.Generic;
using Dirigible.Controllers;
using Dirigible.Interface;
using ModeControllers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Dirigible.Interactable
{
    public class DirigibleInteraction : MonoBehaviour //, MMEventListener<GameStateEvent>
    {
        public LayerMask dirigibleInteractionLayer; // Layer for dirigible objects

        public LayerMask dirigiblePlayerLayer; // Layer for player objects
        [SerializeField] DirigibleAbilityController _dirigibleAbilityController;
        public bool interact;

        [SerializeField] MMFeedbacks dirigibleCrashFeedbacks;

        readonly List<Collider> _interactablesInRange = new();

        DirigibleModeController _dirigibleMasterController;

        bool _interactionBlocked;

        void Start()
        {
            _dirigibleMasterController = GetComponent<DirigibleModeController>();
            if (_dirigibleMasterController == null) Debug.LogError("DirigibleMasterController not found in the scene.");
        }

        void Update()
        {
            if (_interactionBlocked)
                return;

            if (interact)
            {
                PerformInteraction();
                Debug.Log("Dirigible interaction triggered via Update.");
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("SemiPhysicalBoundary"))
                // Don't trigger crash feedbacks for SemiPhysicalBoundary collisions
                return;

            // Get the relative speed at impact
            var crashSpeed = collision.relativeVelocity.magnitude;

            // Convert it to a 0–1 range so you can feed it to MMFeedbacks
            // Adjust maxSpeedThreshold to tune sensitivity
            var maxSpeedThreshold = 50f; // change depending on your game's physics scale
            var crashIntensity = Mathf.Clamp01(crashSpeed / maxSpeedThreshold);

            // Optionally ignore trivial bumps
            if (crashIntensity < 0.1f)
                return;

            dirigibleCrashFeedbacks?.PlayFeedbacks(transform.position, crashIntensity);

            // foreach (var contact in collision.contacts)
            // {
            //     // This is the collider on *your side* of the collision
            //     var thisCol = contact.thisCollider;
            //
            //     // This tells you exactly which child collider was touched
            //     dirigibleCrashFeedbacks?.PlayFeedbacks();
            // }
        }

        void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & dirigibleInteractionLayer) != 0)
                if (!_interactablesInRange.Contains(other))
                    _interactablesInRange.Add(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (_interactablesInRange.Contains(other)) _interactablesInRange.Remove(other);
        }

        // New method to be called directly from input
        public void TriggerInteraction()
        {
            if (_interactionBlocked)
                return;

            PerformInteraction();
        }


        public void DelayInteraction(float delay)
        {
            StartCoroutine(DelayInteractionCoroutine(delay));
        }

        IEnumerator DelayInteractionCoroutine(float delay)
        {
            _interactionBlocked = true;
            yield return new WaitForSeconds(delay);
            _interactionBlocked = false;
        }

        void PerformInteraction()
        {
            foreach (var dirigibleInteractableCollider in _interactablesInRange)
            {
                if (dirigibleInteractableCollider == null)
                {
                    Debug.LogWarning("Collider is null, skipping interaction.");
                    continue;
                }

                var dirigibleInteractable = dirigibleInteractableCollider.GetComponent<IDirigibleInteractable>();
                if (dirigibleInteractable != null)
                {
                    dirigibleInteractable.Interact();
                    return; // Exit after the first interaction
                }

                Debug.LogWarning("DirigibleInteractable was null.");
            }
        }
    }
}
