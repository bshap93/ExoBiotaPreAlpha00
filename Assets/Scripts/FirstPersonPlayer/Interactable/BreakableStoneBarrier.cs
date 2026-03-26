using System;
using System.Collections;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Helpers.Events;
using HighlightPlus;
using Manager.ProgressionMangers;
using Manager.SceneManagers;
using MoreMountains.Feedbacks;
using RayFire;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class BreakableStoneBarrier : MonoBehaviour, IRequiresUniqueID, IBreakable
    {
        [SerializeField] RayfireRigid rayfireRigid;
        [SerializeField] int strengthNeededToBreak = 2;
        public string uniqueId;

        [Tooltip("If set, destroy this root instead of just this component's GameObject.")]
        public GameObject destroyRoot;

        public MMFeedbacks onBreakFeedbacks;
        public MMFeedbacks onHitFeedbacks;
        HighlightEffect _highlightEffect;


        void Awake()
        {
            _highlightEffect = GetComponent<HighlightEffect>();

            if (rayfireRigid == null)
                rayfireRigid = GetComponent<RayfireRigid>();

            rayfireRigid.demolitionEvent.LocalEvent += OnDemolished;
        }


        public bool CanBeDamagedBy(int toolPower, int strength)
        {
            var attrMgr = AttributesManager.Instance;
            return attrMgr != null && attrMgr.Strength >= strengthNeededToBreak;
        }
        public void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal, HitType hitType,
            PlayerAttack attack = null)
        {
            var attrMgr = AttributesManager.Instance;
            var root = destroyRoot != null ? destroyRoot : gameObject;

            if (CanBeDamagedBy(toolPower, attrMgr.Strength))
            {
                foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                if (rayfireRigid != null)
                    rayfireRigid.Demolish();
                else
                    Destroy(root, 0.05f);
            }
            else // Feedback for hitting but not breaking
            {
                onHitFeedbacks?.PlayFeedbacks();
                AlertEvent.Trigger(
                    AlertReason.BreakableToolIneffective,
                    "This tool is ineffective at your current strength level.",
                    "Tool Ineffective");
            }
        }
        public IEnumerator InitializeAfterDestructableManager()
        {
            // Wait one frame so core managers come up
            yield return null;

            // Despawn if already destroyed in this save
            if (DestructableManager.Instance != null &&
                DestructableManager.Instance.IsDestroyed(uniqueId))
                Destroy(gameObject);
        }
        public void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal)
        {
            throw new NotImplementedException();
        }
        public string UniqueID =>
            uniqueId;
        public void SetUniqueID()
        {
            uniqueId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueId);
        }


        void OnDemolished(RayfireRigid demolished)
        {
            if (demolished.HasFragments)
                foreach (var frag in demolished.fragments)
                    frag.gameObject.layer = LayerMask.NameToLayer("Debris");

            onBreakFeedbacks?.PlayFeedbacks();
        }
    }
}
