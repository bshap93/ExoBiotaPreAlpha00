// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Events;
// using Helpers.Events;
// using Helpers.Interfaces;
// using Helpers.ScriptableObjects;
// using MoreMountains.Feedbacks;
// using MoreMountains.Tools;
// using OWPData.ScriptableObjects;
// using SharedUI.Alert;
// using UnityEngine;
//
// namespace Manager.Status
// {
//     public class ContaminationManager : MonoBehaviour, IGameService, MMEventListener<PlayerStatsEvent>,
//         MMEventListener<ContaminationCUEvent>
//     {
//         const string SaveKeyContamination = "ContaminationPoints";
//         const string SaveKeyCU = "ContaminationUnits";
//         [Header("Contamination Units")]
//         [Tooltip("How many contamination points make up one Contamination Unit (CU).")]
//         [SerializeField]
//         float oneCUContamination = 20f;
//
//         [SerializeField] MMFeedbacks thresholdCUFeedback;
//         [SerializeField] string contaminationEffectCatalogId;
//         [SerializeField] string decontaminationEffectCatalogId;
//
//
//         [Tooltip("Autosave player data when dirty.")] [SerializeField]
//         bool autoSave = true;
//
//         [SerializeField] MainTutBitWindowArgs initialContaminationTutorialBit;
//
//         public List<AlertUIController.ModalArgs> modalArgs;
//
//         bool _dirty;
//
//         int _lastCU = -1;
//
//         string _savePath;
//
//
//         public static ContaminationManager Instance { get; private set; }
//
//         public float CurrentContaminationPoints => PlayerModel.Current.contaminationPoints;
//         public int CurrentCU => Mathf.FloorToInt(CurrentContaminationPoints / oneCUContamination);
//         public float CurrentCUFraction => CurrentContaminationPoints % oneCUContamination / oneCUContamination;
//
//         void Awake()
//         {
//             if (Instance == null) Instance = this;
//             else
//                 Destroy(gameObject);
//         }
//
//         void Start()
//         {
//             _savePath = GetSaveFilePath();
//
//             if (!HasSavedData())
//             {
//                 Reset();
//                 return;
//             }
//
//             Load();
//         }
//
//         void OnEnable()
//         {
//             this.MMEventStartListening<PlayerStatsEvent>();
//             this.MMEventStartListening<ContaminationCUEvent>();
//         }
//         void OnDisable()
//         {
//             this.MMEventStopListening<PlayerStatsEvent>();
//             this.MMEventStopListening<ContaminationCUEvent>();
//         }
//         public void ConditionalSave()
//         {
//             if (autoSave && _dirty)
//             {
//                 Save();
//                 _dirty = false;
//             }
//         }
//         public void MarkDirty()
//         {
//             _dirty = true;
//         }
//
//         public string GetSaveFilePath()
//         {
//             return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ContaminationSave);
//         }
//         public void CommitCheckpointSave()
//         {
//             if (_dirty)
//             {
//                 Save();
//                 _dirty = false;
//             }
//         }
//         public bool HasSavedData()
//         {
//             return ES3.FileExists(_savePath ?? GetSaveFilePath());
//         }
//         public void Save()
//         {
//             ES3.Save(SaveKeyContamination, PlayerModel.Current.contaminationPoints, _savePath);
//             ES3.Save(SaveKeyCU, PlayerModel.Current.contaminationCU, _savePath);
//         }
//         public void Load()
//         {
//             if (ES3.KeyExists(SaveKeyContamination, _savePath))
//                 PlayerModel.Current.contaminationPoints = ES3.Load<float>(SaveKeyContamination, _savePath);
//             else
//                 PlayerModel.Current.contaminationPoints = 0f;
//
//             if (ES3.KeyExists(SaveKeyCU, _savePath))
//                 PlayerModel.Current.contaminationCU = ES3.Load<int>(SaveKeyCU, _savePath);
//             else
//                 PlayerModel.Current.contaminationCU = 0;
//
//             NotifyCUChanges();
//         }
//         public void Reset()
//         {
//             PlayerModel.Current.contaminationPoints = 0f;
//             PlayerModel.Current.contaminationCU = 0;
//             _lastCU = -1;
//             _dirty = true;
//             ConditionalSave();
//             NotifyCUChanges();
//         }
//
//         public void OnMMEvent(ContaminationCUEvent eventType)
//         {
//             if (eventType.Type == ContaminationCUEvent.CUEventType.Increased)
//                 switch (eventType.NewCU)
//                 {
//                     case 0:
//                         break;
//                     case 1:
//                         TutorialEvent.Trigger(
//                             initialContaminationTutorialBit.mainTutID, TutorialEventType.ShowMainTutBit);
//
//                         TriggerModalById("CU1Warning");
//
//                         break;
//                     case 2:
//                         TriggerModalById("CU2Warning");
//
//
//                         break;
//                     case 3:
//                         TriggerModalById("CU3Warning");
//                         break;
//                     case 4:
//                         TriggerModalById("CU4Warning");
//                         break;
//                     case 5:
//                         TriggerModalById("CU5Warning");
//                         break;
//                     case 6:
//                         TriggerModalById("CU6Warning");
//                         break;
//                 }
//             else if (eventType.Type == ContaminationCUEvent.CUEventType.Decreased)
//                 AlertEvent.Trigger(
//                     AlertReason.ContaminationWarning,
//                     $"Your contamination level has increased to {eventType.NewCU} CU.",
//                     "Contamination Level Decreased");
//         }
//
//
//         public void OnMMEvent(PlayerStatsEvent e)
//         {
//             if (e.StatType != PlayerStatsEvent.PlayerStat.Contamination) return;
//
//             var signedAmount = e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease
//                 ? -Mathf.Abs(e.Amount)
//                 : Mathf.Abs(e.Amount);
//
//             if (e.OverTime <= 0f)
//             {
//                 PlayerModel.Current.contaminationPoints = Mathf.Max(
//                     0f, PlayerModel.Current.contaminationPoints + signedAmount);
//
//                 NotifyCUChanges();
//                 MarkDirty();
//                 PlayerStatsSyncEvent.Trigger();
//             }
//             else
//             {
//                 if (_contaminationTween != null) StopCoroutine(_contaminationTween);
//                 _contaminationTween = StartCoroutine(TweenContamination(signedAmount, e.OverTime));
//             }
//         }
//
//         public void TriggerModalById(string modalId)
//         {
//             // Find modal data by ID (case-insensitive match)
//             var args = modalArgs.Find(m => string.Equals(m.ID, modalId, StringComparison.OrdinalIgnoreCase));
//             if (args.ID == null)
//             {
//                 Debug.LogWarning($"[ContaminationManager] No ModalArgs found for ID: {modalId}");
//                 return;
//             }
//
//             // Build and trigger the AlertEvent
//             AlertEvent.Trigger(
//                 AlertReason.ContaminationWarning,
//                 args.description,
//                 args.title,
//                 AlertType.ChoiceModal,
//                 alertImage: args.icon,
//                 alertIcon: args.icon,
//                 onConfirm: args.OnConfirm,
//                 onCancel: args.OnCancel
//             );
//         }
//         IEnumerator TweenContamination(float delta, float duration)
//         {
//             var start = PlayerModel.Current.contaminationPoints;
//             var target = Mathf.Max(0f, start + delta);
//             var t = 0f;
//
//             while (t < duration)
//             {
//                 t += Time.deltaTime;
//                 var u = Mathf.Clamp01(t / duration);
//                 var v = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, u));
//                 PlayerModel.Current.contaminationPoints = v;
//                 NotifyCUChanges();
//                 PlayerStatsSyncEvent.Trigger();
//                 yield return null;
//             }
//
//             PlayerModel.Current.contaminationPoints = target;
//             MarkDirty();
//             PlayerStatsSyncEvent.Trigger();
//         }
//
//         // --- Public APIs ---
//         public void AddContamination(float amount, float overTime = 0f)
//         {
//             PlayerStatsEvent.Trigger(
//                 PlayerStatsEvent.PlayerStat.Contamination,
//                 PlayerStatsEvent.PlayerStatChangeType.Increase, amount, overTime);
//         }
//
//         public void ReduceContamination(float amount, float overTime = 0f)
//         {
//             PlayerStatsEvent.Trigger(
//                 PlayerStatsEvent.PlayerStat.Contamination,
//                 PlayerStatsEvent.PlayerStatChangeType.Decrease, amount, overTime);
//         }
//
//         public void SetContamination(float absoluteValue)
//         {
//             PlayerModel.Current.contaminationPoints = Mathf.Max(0f, absoluteValue);
//             MarkDirty();
//             NotifyCUChanges();
//             PlayerStatsSyncEvent.Trigger();
//         }
//
//         public void ClearContamination()
//         {
//             SetContamination(0f);
//         }
//
//         void NotifyCUChanges()
//         {
//             var currentCU = CurrentCU;
//             if (currentCU != _lastCU)
//             {
//                 if (_lastCU >= 0)
//                 {
//                     var type = currentCU > _lastCU
//                         ? ContaminationCUEvent.CUEventType.Increased
//                         : ContaminationCUEvent.CUEventType.Decreased;
//
//                     ContaminationCUEvent.Trigger(currentCU, type);
//                     if (thresholdCUFeedback != null && type == ContaminationCUEvent.CUEventType.Increased)
//                         thresholdCUFeedback.PlayFeedbacks();
//                 }
//
//                 _lastCU = currentCU;
//             }
//         }
//     }
// }


