using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppController_
{
    public class AppController : MonoBehaviour
    {

        [SerializeField] private GameObject menuUi;
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKeyUp(KeyCode.R) && !menuUi.activeSelf)
            {
                var currentScene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(currentScene);
            }
        }
    }
}
