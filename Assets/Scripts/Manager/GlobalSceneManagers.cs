using UnityEngine;

namespace Manager
{
    public class GlobalSceneManagers : MonoBehaviour
    {
        public static GlobalSceneManagers Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // Initialization code can go here if needed
        }

        // Update is called once per frame
        void Update()
        {
            // Update logic can go here if needed
        }
    }
}
