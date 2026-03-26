// SampleDetailsPanel.cs

using System.Text;
using FirstPersonPlayer.ScriptableObjects;
using Manager;
using Manager.Global;
using Manager.SceneManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstPersonPlayer.UI.Samples
{
    public class SamplesListViewElement : MonoBehaviour
    {
        [Header("Basic")] [SerializeField] Image speciesIcon;
        [SerializeField] TMP_Text speciesName;
        [SerializeField] TMP_Text subtitle;

        [Header("Markers")] [SerializeField] TMP_Text markersBlock; // simple multiline text for now
        BioOrganismManager _bioOrganismManager;

        BioSamplesManager _bioSamplesManager;

        void Start()
        {
            _bioSamplesManager = BioSamplesManager.Instance;
            _bioOrganismManager = BioOrganismManager.Instance;
        }

        // TODO: replace with your real analysis gate later
        bool IsSequenced(string speciesId)
        {
            // For now: biologicals are *not recognized* until analysis; always false here.
            // Later: hook to a BiologyAnalysisManager.HasSequenced(speciesId)
            return false;
        }

        public void Bind(BioOrganismSample sample, int carriedCount = 1)
        {
            if (sample == null)
            {
                Clear();
                return;
            }

            if (_bioSamplesManager == null)
                _bioSamplesManager = BioSamplesManager.Instance; // singleton is set in Awake

            if (_bioOrganismManager == null)
                _bioOrganismManager = BioOrganismManager.Instance;

            sample.parentOrgamism = _bioOrganismManager.GetBioOrganismByID(sample.parentOrganismID);

            if (!sample.isKnown)
            {
                speciesName.text = sample.parentOrgamism.organismName;
                speciesIcon.sprite = AssetManager.Instance?.iconRepository.sampleCartridgeIcon;
                subtitle.text = carriedCount > 1
                    ? $"x{carriedCount} — Requires sequencing and analysis"
                    : "Requires sequencing and analysis";

                markersBlock.text = "";
                return;
            }

            // Known → render using the species log
            if (_bioSamplesManager.TryGetSampleLog(sample, out var log))
            {
                speciesName.text = string.IsNullOrEmpty(log.speciesName) ? "Organism" : log.speciesName;
                subtitle.text = $"x{carriedCount} carried";

                var sb = new StringBuilder();
                foreach (var kv in log.markerAmounts) // marker → amount (float)
                    sb.AppendLine($"{kv.Key}: {kv.Value * 100f:0.#}%");

                markersBlock.text = sb.ToString();
            }
        }


        public void Clear()
        {
            speciesName.text = "";
            subtitle.text = "";
            markersBlock.text = "";
            speciesIcon.sprite = null;
        }
    }
}
