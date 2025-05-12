using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public NetworkManager networkManager;
    public GameObject hostButton;
    public GameObject joinButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerListText;
    public GameObject uiCanvas;
    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CheckSession();

    }

   
    public void CheckSession()
    {
        if (networkManager.IsHost)
        {
            HideUI();
            statusText.text = "Hosting Game... Waiting for players.";
        }
      
        else if (networkManager.IsClient)
        {
            hostButton.SetActive(false);
            joinButton.SetActive(true);
            statusText.text = "Join an existing game.";
        }
        else
        {
            ShowUI();
            statusText.text = "Select Host or Join to start.";
        }
    }

    public void UpdatePlayerList(List<PlayerRef> players, Dictionary<PlayerRef, string> playerNames)
    {
        playerListText.text = "Players in Game:\n";
        foreach (var player in players)
        {
            playerListText.text += $"- {playerNames[player]}\n";
        }
    }


    public void OnHostGameButtonClicked()
    {
        networkManager.HostGame();
        HideUI();
        statusText.text = "Hosting Game... Waiting for players.";
    }

    public void OnJoinGameButtonClicked()
    {
        networkManager.JoinSession();
        HideUI();
        statusText.text = "Joining Game...";
    }

    private void HideUI()
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(false);
        else
        {
            hostButton.SetActive(false);
            joinButton.SetActive(false);
        }
    }

    private void ShowUI()
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(true);
        else
        {
            hostButton.SetActive(true);
            joinButton.SetActive(true);
        }
    }

    public void OnDisconnected()
    {
        ShowUI();
        statusText.text = "Disconnected. Please select Host or Join.";
    }
}
