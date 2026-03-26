namespace Interfaces
{
    /// Anything that wants its state saved per-scene implements this.
    public interface ISceneSavable
    {
        /// Called just before the scene this component lives in is unloaded.
        void SaveSceneState(string savePath);

        /// Called right after the scene has finished loading.
        void LoadSceneState(string savePath);
    }
}