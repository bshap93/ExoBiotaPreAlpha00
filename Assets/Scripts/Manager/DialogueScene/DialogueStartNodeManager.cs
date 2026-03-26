using System.Collections.Generic;
using Helpers.Interfaces;
using UnityEngine;

namespace Manager.DialogueScene
{
    // Redundant now that entry nodes can be used within Yarn scripts
    // for given NPCs, 
    // 
    public class DialogueStartNodeManager : MonoBehaviour, ICoreGameService
    {
        const string Key = "DialogueStartNodes";

        [SerializeField] bool autoSave;
        bool _dirty;
        string _savePath;
        Dictionary<string, string> _startNodes;

        public static DialogueStartNodeManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            _savePath = SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.DialogueSave);
            Load();
        }

        public void Save()
        {
            ES3.Save(Key, _startNodes, _savePath);
            _dirty = false;
        }

        public void Load()
        {
            _startNodes = ES3.Load(Key, _savePath, new Dictionary<string, string>());
            _dirty = false;
        }

        public void Reset()
        {
            if (_startNodes == null)
                _startNodes = new Dictionary<string, string>();

            _startNodes.Clear();
            _dirty = true;
            ConditionalSave();
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return _savePath;
        }

        public void CommitCheckpointSave()
        {
            if (autoSave) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath) && ES3.KeyExists(Key, _savePath);
        }


        public string GetStartNode(string npcId, string defaultNode)
        {
            return _startNodes.TryGetValue(npcId, out var node) ? node : defaultNode;
        }

        public void SetStartNode(string npcId, string node)
        {
            _startNodes[npcId] = node;
            _dirty = true;
            ConditionalSave();
        }

        public bool HasSaveData()
        {
            return ES3.FileExists(_savePath) && ES3.KeyExists(Key, _savePath);
        }

    }
}
