using UnityEngine;
using Fusion;
using System.Globalization;

public class ScoreManager : NetworkBehaviour
{
    // Store scores for each player
    private int score = 0;

    // Property to get the current score
    public int Score
    {
        get => score;
        set
        {
            score = value;
            OnScoreChanged?.Invoke(score);
        }
    }

    // Event that will be triggered when the score changes
    public event System.Action<int> OnScoreChanged;

    // Increase score
    public void AddScore(int amount)
    {
        // if (!IsOwner) return; // Only the owner can modify the score
        Score += amount;
    }

    // Decrease score (if needed)
    public void DeductScore(int amount)
    {
        //  if (!IsOwner) return;
        Score -= amount;
    }
}