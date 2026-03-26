// using System;
// using UnityEngine;
// using UnityEngine.Serialization;
// using VTabs.Libs;
//
// namespace OWPData.ScriptableObjects
// {
//     [CreateAssetMenu]
//     public class PlayerModel : ScriptableObject
//     {
//         public float health;
//
//         public float stamina;
//         public float vision;
//
//         [FormerlySerializedAs("contamination")]
//         public float contaminationPoints;
//         public int contaminationCU;
//
//         public float maxHealth;
//         public float maxStamina;
//
//         public float maxVision;
//         public float ContaminationCUFloat => contaminationCU.ToFloat();
//         public static PlayerModel Current { get; set; }
//
//         void Awake()
//         {
//             Current = this;
//             // inside the SO if you like
//         }
//     }
//
//
// //  ❷  DTO  (plain struct that mirrors what you actually want to save)
//     [Serializable]
//     public struct PlayerDto
//     {
//         public float health;
//
//         public float stamina;
//
//         public float vision;
//
//         public float maxHealth;
//
//         public float maxStamina;
//
//         public float maxVision;
//
//         [FormerlySerializedAs("contamination")]
//         public float contaminationPoints;
//
//         public int contaminationCU;
//     }
//
//     //  ❸  Conversion helpers
//     public static class PlayerModelExtensions
//     {
//         public static PlayerDto ToDto(this PlayerModel m)
//         {
//             return new PlayerDto
//             {
//                 health = m.health,
//                 stamina = m.stamina,
//                 contaminationPoints = m.contaminationPoints,
//                 contaminationCU = m.contaminationCU,
//                 maxHealth = m.maxHealth,
//                 maxStamina = m.maxStamina,
//                 vision = m.vision,
//                 maxVision = m.maxVision
//             };
//         }
//
//         public static void FromDto(this PlayerModel m, PlayerDto dto)
//         {
//             m.health = dto.health;
//             m.stamina = dto.stamina;
//             m.contaminationPoints = dto.contaminationPoints;
//             m.contaminationCU = dto.contaminationCU;
//             m.maxHealth = dto.maxHealth;
//             m.maxStamina = dto.maxStamina;
//             m.vision = dto.vision;
//             m.maxVision = dto.maxVision;
//         }
//     }
// }


