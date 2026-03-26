using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.Settings
{
    public class MetadataManager : MonoBehaviour
    {
        [FormerlySerializedAs("GameVersion")] public string gameVersion = "0.0072";
        public string buildDate;
        public static MetadataManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
