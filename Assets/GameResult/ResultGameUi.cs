using System.Collections;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameResult
{
    public class ResultGameUi : NetworkBehaviour
    {
        [SerializeField] private GameObject resultCanvas;
        [SerializeField] private TextMeshProUGUI ResultText;
        [SerializeField] private string resultMessage;
        
        public void GameOver(bool win)
        {
            if(resultCanvas.activeSelf) return;
            resultCanvas.SetActive(true);
            ResultText.text = win ? "You beat them all" : "You lose, next dream";
        }

        private IEnumerator TransitionToMenu()
        {
            yield return new WaitForSeconds(5);
            SceneManager.LoadScene(0);
        }
    }
}