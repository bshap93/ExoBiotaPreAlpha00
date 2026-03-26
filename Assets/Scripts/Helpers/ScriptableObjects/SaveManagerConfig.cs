using System;
using System.Collections.Generic;
using Manager;

namespace Helpers.ScriptableObjects
{
    [Serializable]
    public class SaveManagerConfig
    {
        // Optional: force reset state (e.g. new game start)
        public bool ForceReset;
        // Which global managers should be skipped
        public List<GlobalManagerType> DisabledGlobalManagers = new();
    }
}
