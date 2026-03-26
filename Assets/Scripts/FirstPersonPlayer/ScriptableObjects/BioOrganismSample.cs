using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.ScriptableObjects
{
    [Serializable]
    public class BioLogFile
    {
        public string speciesId; // matches BioOrganismType.organismID
        public string speciesName; // snapshot helper (optional)
        public List<string> sampleIds = new(); // which samples fed this log
        public DateTime lastUpdatedUtc;


        // NEW: “marker -> amount present (0..1 or any unit you choose)”
        public Dictionary<string, float> markerAmounts = new();
        public HashSet<string> symbiosisMarkers = new(); // e.g., "Rhizo-A", "Enzyme-X"
    }

    [Serializable]
    public class BioOrganismSample
    {
        public string uniqueID;

        [FormerlySerializedAs("parentOrgamismID")] [ES3NonSerializable]
        public BioOrganismType parentOrgamism;

        public string parentOrganismID; // snapshot helper (optional)

        public BioLogFile associatedBioLogFile;
        public bool isKnown; // has been analyzed and identified

        public float GetSequencingDuration()
        {
            throw new NotImplementedException();
            // Based on the level of complexity of the organism, 
            // the quality of sequencing gear, and the complexity of the logfile.
        }
    }
}