using Helpers.ScriptableObjects.IconRepositories;
using UnityEngine;

namespace Manager
{
    public class AssetManager : MonoBehaviour
    {
        public IconRepository iconRepository;
        public Sprite defaultUnknownIcon;
        public static AssetManager Instance { get; private set; }

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
    }
}
