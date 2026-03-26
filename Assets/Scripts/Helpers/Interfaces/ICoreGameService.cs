namespace Helpers.Interfaces
{
    public interface ICoreGameService
    {
        public void Save();
        public void Load();
        public void Reset();

        public void ConditionalSave();

        public void MarkDirty();

        public string GetSaveFilePath();


        public void CommitCheckpointSave();

        public bool HasSavedData();
    }
}
