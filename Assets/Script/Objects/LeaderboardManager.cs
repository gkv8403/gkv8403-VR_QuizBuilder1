using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI leaderboardText; // UI Text element to display the leaderboard
    private List<PlayerScore> playerScores = new List<PlayerScore>(); // List to store player scores

    // A class representing a player's score (name and score value)
    [System.Serializable]
    public class PlayerScore
    {
        public string playerName; // Player's name
        public int score;         // Player's score
    }

    // Called at the start of the script
    private void Start()
    {
        // Ensure the leaderboard text UI is assigned
        if (leaderboardText == null)
        {
            Debug.LogError("LeaderboardText is not assigned in the Inspector.");
        }
    }

    /// <summary>
    /// Adds or updates the score of a player in the leaderboard.
    /// </summary>
    /// <param name="playerRef">Reference to the player (PlayerRef) in Fusion</param>
    /// <param name="score">Score to be added to the player</param>
    public void AddPlayerScore(PlayerRef playerRef, int score)
    {
        // Use NetworkManager.Instance for a safe reference
        NetworkManager networkManager = NetworkManager.Instance;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager instance not found.");
            return;
        }

        // Fetch the player's name using the player reference
        string playerName = networkManager.GetPlayerName(playerRef);
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError($"Player name not found for PlayerRef: {playerRef}");
            return;
        }

        // Check if the player already has a score entry
        PlayerScore existingScore = playerScores.Find(p => p.playerName == playerName);

        if (existingScore != null)
        {
            // If the player already exists, update their score
            existingScore.score += score;
        }
        else
        {
            // If the player does not exist, create a new entry
            PlayerScore newScore = new PlayerScore
            {
                playerName = playerName,
                score = score
            };
            playerScores.Add(newScore);
        }

        // Update the leaderboard UI with the latest scores
        UpdateLeaderboard();
    }

    /// <summary>
    /// Updates the leaderboard text UI with the current player scores.
    /// </summary>
    private void UpdateLeaderboard()
    {
        // Sort the player scores in descending order (highest score first)
        playerScores.Sort((x, y) => y.score.CompareTo(x.score));

        // Initialize the leaderboard text
        leaderboardText.text = "Leaderboard:\n";

        // Add each player's name and score to the leaderboard display
        foreach (var playerScore in playerScores)
        {
            leaderboardText.text += $"{playerScore.playerName}: {playerScore.score}\n";
        }
    }
}
