using Helpers.Interfaces;
using UnityEngine;

namespace Manager
{
    public abstract class GameManagerAbstract<T> : MonoBehaviour, ICoreGameService
        where T : GameManagerAbstract<T>
    {
        public bool autoSave;
        // Whether there are unsaved changes
        protected bool Dirty;
        // Path to the save file
        protected string SavePath;

        // public static GameManagerAbstract Instance { get; private set; }

        #region BoilerPlate LifeCycle Methods

        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
                Instance = (T)this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            SavePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[TriggerColliderManager] No save file found; starting with defaults.");
                Reset();
            }

            Load();
        }

        protected abstract void OnEnable();

        protected abstract void OnDisable();

        #endregion

        #region Save Load Reset Methods

        public abstract void Save();

        public abstract void Load();
        public virtual void Reset()
        {
            // Implement reset logic here

            Dirty = true;
            ConditionalSave();
        }
        public void ConditionalSave()
        {
            if (autoSave && Dirty) Save();
        }
        public void MarkDirty()
        {
            Dirty = true;
        }
        public abstract string GetSaveFilePath();

        public void CommitCheckpointSave()
        {
            if (!Dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(SavePath ?? GetSaveFilePath());
        }

        #endregion
    }
}
