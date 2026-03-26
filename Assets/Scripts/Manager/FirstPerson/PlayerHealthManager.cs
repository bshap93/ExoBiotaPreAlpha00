using Domains.Player.Events;
using FirstPersonPlayer.UI;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using ScriptableObjects;
using UnityEngine;

namespace Domains.Player.Scripts
{
    // Obsolete: This class is now replaced by PlayerStats
    public class PlayerHealthManager : MonoBehaviour, MMEventListener<HealthEvent>
    {
        public const string SaveFileName = "GameSave.es3";
        public static float HealthPoints;
        public static float MaxHealthPoints;


        public static float InitialCharacterHealth;

        // Add flag to prevent multiple death triggers
        static bool isDead;
        public HealthBarUpdater healthBarUpdater;

        public MMFeedbacks lavaDamageFeedbacks;
        public MMFeedbacks fallDamageFeedbacks;


        public bool immuneToDamage;

        CharacterStatProfile _characterStatProfile;

        string _savePath;

        void Awake()
        {
            if (healthBarUpdater == null)
            {
                healthBarUpdater = FindFirstObjectByType<HealthBarUpdater>();
                if (healthBarUpdater == null)
                    Debug.LogError("PlayerHealthManager: No HealthBarUpdater found in scene!");
            }

            _characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (_characterStatProfile != null)
                InitialCharacterHealth = _characterStatProfile.InitialMaxHealth;
            else
                Debug.LogError("CharacterStatProfile not set in PlayerHealthManager");
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("[PlayerHealthManager] No save file found, forcing initial save...");
                ResetPlayerHealth(); // Ensure default values are set
            }

            LoadPlayerHealth();
        }


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(HealthEvent eventType)
        {
            switch (eventType.EventType)
            {
                case HealthEventType.ConsumeHealth:
                    ConsumeHealth(eventType.ByValue, eventType.Reason);
                    break;
                case HealthEventType.RecoverHealth:
                    RecoverHealth(eventType.ByValue);
                    break;
                case HealthEventType.IncreaseMaximumHealth:
                    IncreaseMaximumHealth(eventType.ByValue);
                    break;
                case HealthEventType.DecreaseMaximumHealth:
                    DecreaseMaximumHealth(eventType.ByValue);
                    break;
                case HealthEventType.FullyRecoverHealth:
                    FullyRecoverHealth();
                    break;
                case HealthEventType.SetCurrentHealth:
                    SetCurrentHealth(eventType.ByValue);
                    break;
                default:
                    Debug.LogWarning($"Unknown HealthEventType: {eventType.EventType}");
                    break;
            }
        }


        public void Initialize()
        {
            ResetPlayerHealth();
            healthBarUpdater.Initialize();
        }

        public void ConsumeHealth(float healthToConsume, HealthEventReason? reason = null)
        {
            if (immuneToDamage) return;

            // Don't process damage if already dead
            if (isDead) return;

            // Store the previous health
            var previousHealth = HealthPoints;

            if (HealthPoints - healthToConsume <= 0)
            {
                HealthPoints = 0;
                isDead = true; // Set dead flag to prevent multiple triggers
                PlayerStatusEvent.Trigger(PlayerStatusEventType.Died, reason);
            }
            else
            {
                if (reason == null)
                {
                    HealthPoints -= healthToConsume;
                }
                else
                {
                    switch (reason)
                    {
                        case HealthEventReason.FallDamage:
                            fallDamageFeedbacks?.PlayFeedbacks();
                            break;
                        case HealthEventReason.LavaDamage:
                            lavaDamageFeedbacks?.PlayFeedbacks();
                            break;
                        default:
                            Debug.LogWarning($"Unknown HealthEventReason: {reason}");
                            break;
                    }

                    HealthPoints -= healthToConsume;
                }
            }

            // Make sure the UI gets updated by triggering a SetCurrentHealth event if health changed
            if (!Mathf.Approximately(previousHealth, HealthPoints))
                HealthEvent.Trigger(HealthEventType.SetCurrentHealth, HealthPoints);


            // SavePlayerHealth();
        }

        public static void RecoverHealth(float amount)
        {
            if (HealthPoints == 0 && amount > 0)
            {
                PlayerStatusEvent.Trigger(PlayerStatusEventType.RegainedHealth);
                isDead = false; // Reset dead flag when health is regained
            }

            var newHealth = HealthPoints + amount;
            if (newHealth > MaxHealthPoints)
                HealthPoints = MaxHealthPoints;
            else
                HealthPoints = newHealth;
            // SavePlayerHealth();
        }

        public static void FullyRecoverHealth()
        {
            HealthPoints = MaxHealthPoints;
            isDead = false; // Reset dead flag when health is fully recovered
            PlayerStatusEvent.Trigger(PlayerStatusEventType.RegainedHealth);
        }

        public static void SetCurrentHealth(float amount)
        {
            HealthPoints = amount;
        }

        // Method to reset the dead flag - should be called after teleporting player to safety
        public static void ResetDeadFlag()
        {
            isDead = false;
        }


        public static void IncreaseMaximumHealth(float amount)
        {
            MaxHealthPoints += amount;
        }

        public static void DecreaseMaximumHealth(float amount)
        {
            MaxHealthPoints -= amount;
        }


        static string GetSaveFilePath()
        {
            return SaveFileName;
        }

        public void LoadPlayerHealth()
        {
            var saveFilePath = GetSaveFilePath();

            if (ES3.FileExists(saveFilePath))
            {
                HealthPoints = ES3.Load<float>("HealthPoints", saveFilePath);
                MaxHealthPoints = ES3.Load<float>("MaxHealthPoints", saveFilePath);
                isDead = HealthPoints <= 0;
                healthBarUpdater.Initialize();
            }
            else
            {
                ResetPlayerHealth();
                healthBarUpdater.Initialize();
            }
        }

        public static void ResetPlayerHealth()
        {
            var characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (characterStatProfile == null)
            {
                Debug.LogError("CharacterStatProfile not found! Using default values.");
                HealthPoints = 20f; // Default fallback
                MaxHealthPoints = 20f;
            }
            else
            {
                HealthPoints = characterStatProfile.InitialMaxHealth;
                MaxHealthPoints = characterStatProfile.InitialMaxHealth;
            }

            isDead = false; // Reset dead flag on health reset

            PlayerStatusEvent.Trigger(PlayerStatusEventType.ResetHealth);
        }


        public static void SavePlayerHealth()
        {
            ES3.Save("HealthPoints", HealthPoints, "GameSave.es3");
            ES3.Save("MaxHealthPoints", MaxHealthPoints, "GameSave.es3");
        }


        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath());
        }
    }
}
