using System;
using Fusion;
using Spawner;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuGame
{
  public class MenuHandlerUi : MonoBehaviour
  {
    [SerializeField] private TMP_InputField playerNickname;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private Button hostGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private BasicSpawner _basicSpawner;
    [SerializeField] private GameObject menu;

    private string roomName;
    private void Start()
    {
      playerNickname.onValueChanged.AddListener(OnPlayerNicknameChange);
      roomNameInputField.onValueChanged.AddListener(OnRoomNameChange);
      hostGameButton.onClick.AddListener(OnHostGameButtonPressed);
      joinGameButton.onClick.AddListener(OnJoinButtonPressed);
    }

    private void OnJoinButtonPressed()
    {
      _basicSpawner.StartGame(GameMode.Client, roomName);
      menu.SetActive(false);
    }

    private void OnHostGameButtonPressed()
    {
      _basicSpawner.StartGame(GameMode.Host, roomName);
      menu.SetActive(false);
    }

    private void OnRoomNameChange(string roomName)
    {
        this.roomName = roomName;
    }

    private void OnPlayerNicknameChange(string nickname)
    {
      PlayerPrefs.SetString("playerNickname", nickname);
    }
  }
}
