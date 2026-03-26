using Manager;
using UnityEngine;

namespace CamerasD
{
    public class MainCamera : MonoBehaviour
    {
        public static MainCamera Instance;

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
