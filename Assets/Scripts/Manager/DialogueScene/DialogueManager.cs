using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.UI.LocationButtonBase.Test;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.NPCs;
using Helpers.Events.UI;
using Interfaces;
using LevelConstruct.Interactable.ItemInteractables;
using Manager.Global;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives;
using Overview.NPC;
using Overview.UI;
using OWPData.Structs;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Yarn.Unity;

namespace Manager.DialogueScene
{
    public class DialogueManager : MonoBehaviour, IGameMetaService, MMEventListener<SpecialDialogueEvent>
    {
        [SerializeField] Camera stageCamera; // drag StageCamera from DialogueOverlay
        [SerializeField] Transform stageRoot; // drag StageRoot from DialogueOverlay
        [SerializeField] CanvasGroup overlay; // fade & block clicks
        [SerializeField] NPCUIIdentifierPanel npcUIIdentifierPanel; // for showing NPC name
        public DialogueRunner dialogueRunner;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int dialogueActionId;

        [SerializeField] bool autoSave;

        [SerializeField] NpcDatabase npcDatabase;

        [SerializeField] RawImage avatarImage;
        [FormerlySerializedAs("interfaceBackground")] [SerializeField]
        GameObject nonNPCInterface;

        [SerializeField] MMFeedbacks startInFirstPersonDialogueFeedbacks;

        [SerializeField] VarProbeYSES3 varProbeYSES3; // For testing purposes, remove later

        // Referenced here to give a central place to access the variable storage
        public VariableStorageBehaviour variableStorage;

        [SerializeField] YarnInitScriptSet[] initScriptSets;

        public CanvasGroup avatarUIElement;
        GameObject _currentModel;


        bool _dirty;

        public bool IsDialogueActive => dialogueRunner.IsDialogueRunning;

        public static DialogueManager Instance { get; private set; }


        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            // init static classes
            FirstPersonStaticDialogueHelpers.Initialize(startInFirstPersonDialogueFeedbacks, dialogueActionId);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void Reset()
        {
            DialogueStartNodeManager.Instance.Reset();
            DialogueVariableManager.Instance.Reset();
            RunInitScripts();

            Debug.Log("DialogueManager: Reset called. All dialogue data cleared.");
            _dirty = true;
            ConditionalSave();
        }

        public void Save()
        {
            DialogueStartNodeManager.Instance.Save();
            DialogueVariableManager.Instance.Save();
            _dirty = false;
        }

        public void Load()
        {
            DialogueStartNodeManager.Instance.Load();
            DialogueVariableManager.Instance.Load();
            _dirty = false;

            if (!DialogueVariableManager.Instance.HasSavedVariables()) RunInitScripts();
        }
        public void OnMMEvent(SpecialDialogueEvent eventType)
        {
            if (eventType.EventType == SpecialDialogueEventType.RequestSpecialDialogue)
            {
                if (eventType.SpecialDialogueType == SpecialDialogueType.MockConsoleDataWindow)
                    // TriggerInfoSequence();
                    Debug.Log("SpecialDialogueEvent: RequestSpecialDialogue for MockConsoleDataWindow received.");
                else
                    Debug.LogWarning(
                        $"SpecialDialogueEvent: Unknown SpecialDialogueType {eventType.SpecialDialogueType} received.");
            }
        }

        public void OnStartDialogue()
        {
            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, 0);
        }

        public void OnEndDialogue()
        {
            ControlsHelpEvent.Trigger(ControlHelpEventType.Show, dialogueActionId);
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }

        public void RunInitScripts()
        {
            foreach (var scriptSet in initScriptSets)
            {
                if (scriptSet == null || scriptSet.yarnProject == null)
                    continue;

                dialogueRunner.SetProject(scriptSet.yarnProject);
                foreach (var nodeName in scriptSet.nodesToRun)
                    if (!string.IsNullOrEmpty(nodeName))
                    {
                        Debug.Log($"Running Yarn init node: {nodeName}");
                        dialogueRunner.StartDialogue(nodeName);
                    }
            }
        }


        void Close()
        {
            // Always release dialogue camera focus, regardless of how dialogue ended.
            DialogueCameraEvent.Trigger(DialogueCameraEventType.ReleaseFocus);
            if (_currentModel) Destroy(_currentModel);
            MyUIEvent.Trigger(UIType.Dialogue, UIActionType.Close);

            dialogueRunner.Stop();
            if (overlay != null)
            {
                overlay.blocksRaycasts = false;
                overlay.interactable = false;
                overlay.alpha = 0;
            }

            HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ShowHotbars);
            DialogueEvent.Trigger(DialogueEventType.DialogueFinished, null, null);

            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, 0);
        }

        public async void OpenNPCDialogue(NpcDefinition def, Transform camAnchor = null, bool autoClose = true,
            string startNodeOverride = null)
        {
            // Use RawImage with Avatar
            avatarImage.gameObject.SetActive(true);
            nonNPCInterface.SetActive(false);
            // 1) put NPC in the stage
            if (def.hasAvatarDiorama)
            {
                _currentModel = Instantiate(def.characterPrefab, stageRoot);
                _currentModel.transform.localPosition = Vector3.zero;
                _currentModel.transform.localRotation = Quaternion.identity;
            }

            npcUIIdentifierPanel.SetInfo(def.characterName);


            // 3) push dialogue
            dialogueRunner.SetProject(def.yarnProject);

            var startNodeStr = startNodeOverride ?? def.startNode;
            var startNodeToUse = DialogueStartNodeManager.Instance.GetStartNode(def.npcId, startNodeStr);
            DialogueEvent.Trigger(DialogueEventType.DialogueStarted, def.npcId, startNodeToUse);
            dialogueRunner.StartDialogue(startNodeToUse);

            // 4) show overlay
            overlay.alpha = 1;
            overlay.blocksRaycasts = true;
            overlay.interactable = true;

            if (!def.hasAvatarDiorama)
            {
                avatarUIElement.alpha = 0;
                avatarUIElement.blocksRaycasts = false;
                avatarUIElement.interactable = false;
            }
            else
            {
                avatarUIElement.alpha = 1;
                avatarUIElement.blocksRaycasts = true;
                avatarUIElement.interactable = true;
            }

            HotbarEvent.Trigger(HotbarEvent.HotbarEventType.HideHotbars);

            // MyUIEvent.Trigger(UIType.Dialogue, UIActionType.Open);

            // 5) WAIT here until all Yarn nodes (and any option UIs) finish
            await dialogueRunner.DialogueTask; // ‑‑ this replaces WaitForDialogueToFinish()

            if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson)
            {
                if (autoClose) // only close if caller asked for it
                    Close();
            }
            else
            {
                // TriggerRetreatFromLocationEvent();

                varProbeYSES3.TryGet();

                if (autoClose) // only close if caller asked for it
                    Close();
            }
        }


        public static string[] GetAllNpcIdOptions()
        {
#if UNITY_EDITOR
            // In editor, find the DialogueManager in the scene
            if (!Application.isPlaying)
            {
                var manager = FindFirstObjectByType<DialogueManager>();
                if (manager?.npcDatabase != null)
                    return manager.npcDatabase.GetAllNpcIds();
            }
#endif

            // At runtime, use the instance
            if (Instance?.npcDatabase != null)
                return Instance.npcDatabase.GetAllNpcIds();

            return new[] { "NpcDatabase not assigned!" };
        }

        public static string[] GetNpcStartNodesByNpcId(string npcId)
        {
#if UNITY_EDITOR
            // In editor, find the DialogueManager in the scene
            if (!Application.isPlaying)
            {
                var manager = FindFirstObjectByType<DialogueManager>();
                if (manager?.npcDatabase != null)
                    return manager.npcDatabase.GetStartNodesForNpc(npcId);
            }
#endif

            // At runtime, use the instance
            if (Instance?.npcDatabase != null)
                return Instance.npcDatabase.GetStartNodesForNpc(npcId);

            return new string[] { };
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif
    }
}
