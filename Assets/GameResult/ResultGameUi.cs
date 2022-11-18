using Fusion;
using TMPro;
using UnityEngine;

namespace GameResult
{
    public class ResultGameUi : NetworkBehaviour
    {
        [SerializeField] private GameObject resultCanvas;
        [SerializeField] private TextMeshProUGUI ResultText;
        [SerializeField] private string resultMessage;
        
        public void GameOver(bool win)
        {
            resultCanvas.SetActive(true);
            ResultText.text = win ? "You beat them all" : "You lose, next dreem";
        }
    }
}