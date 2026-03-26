using Helpers.Interfaces;
using UnityEngine;
using Yarn.Unity;

namespace Manager.DialogueScene
{
    public class DialogueVariableManager : MonoBehaviour, ICoreGameService
    {
        public static DialogueVariableManager Instance;
        public VariableStorageBehaviour dialogueVariableStorage;

        [SerializeField] bool autoSave; // <— NEW
        bool _dirty; // <— NEW

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Save()
        {
            // If ES3VariableStorage is assigned, persist to disk
            if (dialogueVariableStorage is ES3VariableStorage es3)
                // optional: expose a Flush method on ES3VariableStorage
            {
                es3.SaveAllVariables();
                _dirty = false;
            }
        }

        public void Load()
        {
            if (dialogueVariableStorage is ES3VariableStorage es3) es3.ReloadFromDisk();
            // InMemory storage doesn’t persist, so Load would just be a no-op
            _dirty = false;
        }

        public void Reset()
        {
            dialogueVariableStorage.Clear();
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
            if (dialogueVariableStorage is ES3VariableStorage es3) return es3.GetFilePath(); // if you expose it
            return "(in-memory)";
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return HasSavedVariables();
        }


        public bool HasSavedVariables()
        {
            if (dialogueVariableStorage is ES3VariableStorage es3)
            {
                var path = es3.GetFilePath();
                return ES3.FileExists(path) &&
                       (ES3.KeyExists("Yarn.Float", path) ||
                        ES3.KeyExists("Yarn.String", path) ||
                        ES3.KeyExists("Yarn.Bool", path));
            }

            return false;
        }
    }
}
