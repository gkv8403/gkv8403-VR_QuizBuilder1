using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // Reference to the NetworkManager (for managing host/join operations)
    public NetworkManager networkManager;

    // UI Elements
    public TextMeshProUGUI statusText;      // Displays the current connection status (e.g., Hosting, Joining)
    public TextMeshProUGUI playerListText;  // Displays the list of players in the room
    public GameObject uiCanvas;             // Main UI Canvas for hosting/joining options

    // Singleton instance of UIManager for easy access
    public static UIManager Instance;

    // Called when the script instance is being loaded
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Updates the player list UI with the names of connected players.
    /// </summary>
    /// <param name="players">List of connected players (PlayerRef)</param>
    /// <param name="playerNames">Dictionary mapping PlayerRef to player names</param>
    public void UpdatePlayerList(List<PlayerRef> players, Dictionary<PlayerRef, string> playerNames)
    {
        // Clear the player list text
        playerListText.text = "Players in Room:\n";

        // Loop through the list of players
        foreach (var player in players)
        {
            // Check if the player has a name in the dictionary
            if (playerNames.TryGetValue(player, out string playerName))
            {
                // Display player name
                playerListText.text += $"{playerName}\n";
            }
            else
            {
                // Display a default name if player name is not found
                playerListText.text += $"Player_{player.PlayerId} (Unnamed)\n";
            }
        }
    }

   

    /// <summary>
    /// Hides the main UI canvas (used when hosting or joining a game).
    /// </summary>
    private void HideUI()
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(false); // Deactivate the UI Canvas
    }

    /// <summary>
    /// Shows the main UI canvas (used when disconnected).
    /// </summary>
    private void ShowUI()
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(true); // Activate the UI Canvas
    }

    /// <summary>
    /// Handles UI changes when the player is disconnected from the network.
    /// </summary>
    public void OnDisconnected()
    {
        ShowUI(); // Show the main UI
        statusText.text = "Disconnected. Please select Host or Join."; // Update status text
    }
}
