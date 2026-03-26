

// for DestructableEvent (optional, matches your ore node usage)

namespace FirstPersonPlayer.Interactable
{
    // [DisallowMultipleComponent]
    // public class HatchetBreakable : MonoBehaviour //, IBreakable
    // {
    // [FormerlySerializedAs("hitsToBreak")]
    // [Header("Break Settings")]
    // [Tooltip("How many successful hatchet hits until this is destroyed.")]
    // public int defaultHitsToBreak = 2;
    //
    // [Tooltip("Minimum tool power required to count as a successful hit.")]
    // public int hardness = 1;
    //
    // [Tooltip("If set, destroy this root instead of just this component's GameObject.")]
    // public GameObject destroyRoot;
    //
    // [Tooltip("If true, Destroy() the object. If false, just disable renderers/colliders.")]
    // public bool destroyGameObject = true;

    // [Header("FX")] public MMFeedbacks onHitFeedbacks;

    // public MMFeedbacks onBreakFeedbacks;
    // public GameObject hitParticles;
    // public GameObject breakParticles;

    // [SerializeField] BioOrganismBreakableNode bioOrganismNode;
    //
    //
    // [FormerlySerializedAs("_rf")] [SerializeField]
    // RayfireRigid rf;
    //
    // HighlightEffect _highlight; // cache HighlightEffect if present
    //
    // int _hitCount;
    // bool _isBroken; // Prevent multiple breaks

    // void Awake()
    // {
    //     _highlight = GetComponent<HighlightEffect>();
    //     rf = GetComponent<RayfireRigid>();
    //
    //
    //     rf.demolitionEvent.LocalEvent += OnDemolished;
    // }


    // public bool CanBeDamagedBy(int toolPower, int strength)
    // {
    //     return toolPower >= hardness;
    // }
    // public void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal)
    // {
    //     ApplyHatchetHit(toolPower, hitPoint, hitNormal);
    // }
    //

    // void OnDemolished(RayfireRigid demolished)
    // {
    //     if (demolished.HasFragments)
    //         foreach (var frag in demolished.fragments)
    //             frag.gameObject.layer = LayerMask.NameToLayer("Debris");
    // }

    // void ApplyHatchetHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal)
    // {
    //     var attrMgr = AttributesManager.Instance;
    //     // Prevent breaking if already broken
    //     if (_isBroken)
    //     {
    //         Debug.LogWarning($"HatchetBreakable [{bioOrganismNode.uniqueID}]: Already broken, ignoring hit");
    //         return;
    //     }
    //
    //     if (!CanBeDamagedBy(toolPower, 0))
    //     {
    //         PlayHitFx(hitPoint, hitNormal);
    //         return;
    //     }
    //
    //     var actualHitsToBreak = defaultHitsToBreak;
    //
    //     switch (attrMgr.Strength)
    //     {
    //         case 1:
    //             break;
    //         default:
    //             // 4/5^(level-1) of the hits rounded up
    //             actualHitsToBreak = Mathf.RoundToInt(defaultHitsToBreak * Mathf.Pow(0.8f, attrMgr.Strength - 1));
    //             actualHitsToBreak = Mathf.Max(1, actualHitsToBreak);
    //             break;
    //     }
    //
    //     _hitCount++;
    //     PlayHitFx(hitPoint, hitNormal);
    //
    //     if (_hitCount < actualHitsToBreak) return;
    //
    //     _isBroken = true;
    //
    //     // break FX
    //     onBreakFeedbacks?.PlayFeedbacks(transform.position);
    //     if (breakParticles)
    //     {
    //         var fx2 = Instantiate(breakParticles, transform.position, Quaternion.identity);
    //         Destroy(fx2, 3f);
    //     }
    //
    //     if (!string.IsNullOrEmpty(bioOrganismNode.uniqueID))
    //         DestructableEvent.Trigger(DestructableEventType.Destroyed, bioOrganismNode.uniqueID, transform);
    //
    //     var root = destroyRoot != null ? destroyRoot : gameObject;
    //     if (destroyGameObject)
    //     {
    //         foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
    //         foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
    //         if (rf != null)
    //             rf.Demolish();
    //         else
    //             Destroy(root, 0.05f);
    //     }
    //     else
    //     {
    //         foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
    //         foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
    //         enabled = false;
    //     }
    //
    //     ControlsHelpEvent.Trigger(ControlHelpEventType.ShowUseThenHide, 54);
    // }

    // public void BreakInstantly()
    // {
    //     if (_isBroken)
    //     {
    //         Debug.LogWarning(
    //             $"HatchetBreakable [{bioOrganismNode.uniqueID}]: Already broken, ignoring BreakInstantly call");
    //
    //         return;
    //     }
    //
    //     // Skip the incremental hits; just perform the full break logic
    //     _hitCount = defaultHitsToBreak;
    //     ApplyHatchetHit(hardness, transform.position, transform.up);
    // }

    // void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal)
    // {
    //     onHitFeedbacks?.PlayFeedbacks(transform.position);
    //
    //     if (hitParticles)
    //     {
    //         var fx = Instantiate(hitParticles, hitPoint, Quaternion.LookRotation(hitNormal));
    //         Destroy(fx, 2f);
    //     }
    //
    //     // Highlight Plus Hit FX
    //     if (_highlight != null) _highlight.HitFX(); // plays the configured hit effect
    // }
    // }
}
