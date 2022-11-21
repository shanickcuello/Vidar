using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppController_
{
    public class AppController : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                var currentScene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(currentScene);
            }
        }
    }
}
