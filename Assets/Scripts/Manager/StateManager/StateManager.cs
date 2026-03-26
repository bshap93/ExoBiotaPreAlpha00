using UnityEngine;

namespace Manager.StateManager
{
    public abstract class StateManager<T> : MonoBehaviour where T : StateManager<T>
    {
        public bool autoSave;

        protected bool Dirty;
        protected string SavePath;
        public static T Instance { get; private set; }
        protected void Awake()
        {
            if (Instance == null)
                Instance = (T)this;
            else
                Destroy(gameObject);
        }
        public abstract void Reset();

        public abstract void Save();
        public abstract void Load();

        protected void MarkDirty()
        {
            Dirty = true;
        }

        protected abstract string GetSaveFilePath();

        protected void ConditionalSave()
        {
            if (autoSave && Dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(SavePath ?? GetSaveFilePath());
        }
    }
}
