using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class MCQManager : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI questionText; // Text element to display the question
    [SerializeField] private Button[] answerButtons;       // Array of buttons for answer options
    [SerializeField] private ScoreManager scoreManager;    // Reference to ScoreManager for tracking score
    [SerializeField] private LeaderboardManager leaderboardManager; // Reference to LeaderboardManager for leaderboard updates
    public NetworkManager networkManager;                  // Reference to the NetworkManager for player management

    [Header("Quiz Settings")]
    [SerializeField] private QuestionDatabase questionDatabase; // Database containing all quiz questions
    private int currentQuestionIndex = 0;                       // Index of the current question

    // Called when the script starts
    private void Start()
    {
        LoadQuestion(); // Load the first question
    }

    // Loads the current question and sets the UI
    private void LoadQuestion()
    {
        // Ensure the question database is available and has questions
        if (questionDatabase == null || questionDatabase.questions.Length == 0)
        {
            Debug.LogError("[MCQ] No questions found in the database.");
            return;
        }

        // Get the current question based on index
        Question question = questionDatabase.questions[currentQuestionIndex];
        questionText.text = question.questionText; // Set question text in UI

        // Loop through answer buttons and set their text
        for (int i = 0; i < answerButtons.Length; i++)
        {
            // Ensure the button has a corresponding answer
            if (i < question.answers.Length)
            {
                answerButtons[i].GetComponentInChildren<Text>().text = question.answers[i]; // Set answer text

                int answerIndex = i; // Local copy of the index to avoid closure issues in the lambda function

                // Set up the button click event
                answerButtons[i].onClick.RemoveAllListeners(); // Clear previous listeners
                answerButtons[i].onClick.AddListener(() => SubmitAnswer(answerIndex)); // Add new listener

                answerButtons[i].gameObject.SetActive(true); // Make sure the button is visible
            }
            else
            {
                // Hide unused buttons
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // Called when an answer button is clicked
    public void SubmitAnswer(int answerIndex)
    {
        // Get the current question
        Question question = questionDatabase.questions[currentQuestionIndex];

        // Check if the selected answer is correct
        if (answerIndex == question.correctOptionIndex)
        {
            Debug.Log("[MCQ] Correct answer selected!");
            scoreManager.AddScore(10); // Award 10 points for a correct answer
        }
        else
        {
            Debug.Log("[MCQ] Incorrect answer selected.");
        }

        // Update the leaderboard with the player's new score
        leaderboardManager.AddPlayerScore(networkManager.runner.LocalPlayer, scoreManager.Score);

        // Load the next question
        NextQuestion();
    }

    // Loads the next question in the database
    private void NextQuestion()
    {
        currentQuestionIndex++; // Move to the next question

        // Check if there are more questions
        if (currentQuestionIndex >= questionDatabase.questions.Length)
        {
            Debug.Log("[MCQ] Quiz completed.");
            return; // End the quiz if there are no more questions
        }

        // Load the next question
        LoadQuestion();
    }
}
