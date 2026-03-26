using UnityEngine;
using UnityEngine.SceneManagement;

namespace SharedUI
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] GameObject continueButton;


        void Start()
        {
            // Hide Continue by default
            if (continueButton != null)
                continueButton.SetActive(HasSaveData());
        }
        public void OnNewGame()
        {
            // Force a reset so SaveManager clears all data
            BootLoader.ForceNewGame = true;
            SceneManager.LoadScene("Boot");
        }

        public void OnContinue()
        {
            BootLoader.ForceNewGame = false;
            SceneManager.LoadScene("Boot");
        }

        public void OnQuit()
        {
            Application.Quit();
        }

        bool HasSaveData()
        {
            return true;
        }
    }
}
