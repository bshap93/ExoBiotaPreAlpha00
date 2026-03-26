using System.Collections.Generic;
using Helpers.Events;
using INab.Dissolve;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class MaterialsManager : MonoBehaviour, MMEventListener<MaterialsEvent>
    {
        public static MaterialsManager Instance;
        static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        [SerializeField] Dissolver dissolver;

        public List<Material> dissolvableMaterials;

        readonly Dictionary<string, Material> _materials = new();
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        void Start()
        {
            foreach (var mat in dissolvableMaterials)
                _materials.TryAdd(mat.name, mat);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MaterialsEvent eventType)
        {
        }
    }
}
