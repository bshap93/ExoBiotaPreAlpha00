using Domains.Player.Scripts;
using OWPData.ScriptableObjects;
using UnityEngine;

namespace FirstPersonPlayer
{
    public class PlayerInfoSheet : MonoBehaviour
    {
        public static int WeightLimit;

        CharacterStatProfile initialStats;
        static PlayerInfoSheet Instance { get; set; }


        void Awake()
        {
            initialStats = Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (initialStats != null)
                WeightLimit = initialStats.InitialWeightLimit;
            else
                Debug.LogError("CharacterStatProfile not set in PlayerInfoSheet");
        }
    }
}
