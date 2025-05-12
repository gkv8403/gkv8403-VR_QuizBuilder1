using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI leaderboardText;
    private List<PlayerScore> playerScores = new List<PlayerScore>();
    public NetworkManager networkManager;
    [System.Serializable]
    public class PlayerScore
    {
        public string playerName;
        public int score;
    }

    public void AddPlayerScore(PlayerRef playerRef, int score)
    {
        // Fetch the player's name from the NetworkManager
        string playerName = networkManager.GetPlayerName(playerRef);

        PlayerScore newScore = new PlayerScore { playerName = playerName, score = score };
        playerScores.Add(newScore);
        UpdateLeaderboard();
    }

    private void UpdateLeaderboard()
    {
        playerScores.Sort((x, y) => y.score.CompareTo(x.score)); // Sort by score in descending order
        leaderboardText.text = "Leaderboard:\n";

        foreach (var playerScore in playerScores)
        {
            leaderboardText.text += $"{playerScore.playerName}: {playerScore.score}\n";
        }
    }
}
