using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class MCQManager : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LeaderboardManager leaderboardManager;
    public NetworkManager networkManager;
    [Header("Quiz Settings")]
    [SerializeField] private QuestionDatabase questionDatabase;
    private int currentQuestionIndex;

    private void Start()
    {
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        if (questionDatabase == null || questionDatabase.questions.Length == 0)
        {
            Debug.LogError("[MCQ] No questions found in the database.");
            return;
        }

        Question question = questionDatabase.questions[currentQuestionIndex];
        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.answers.Length)
            {
                answerButtons[i].GetComponentInChildren<Text>().text = question.answers[i];
                int answerIndex = i; // Local copy for the button click event
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => SubmitAnswer(answerIndex));
                answerButtons[i].gameObject.SetActive(true);
            }
           
        }
    }

    public void SubmitAnswer(int answerIndex)
    {
        Question question = questionDatabase.questions[currentQuestionIndex];
        if (answerIndex == question.correctOptionIndex)
        {
            Debug.Log("[MCQ] Correct answer selected!");
            // Add points for the correct answer
            scoreManager.AddScore(10); // Add 10 points for correct answer
        }
        else
        {
            Debug.Log("[MCQ] Incorrect answer selected.");
        }
        leaderboardManager.AddPlayerScore(networkManager.runner.LocalPlayer, scoreManager.Score);
        NextQuestion();

    }

    private void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex >= questionDatabase.questions.Length)
        {
            Debug.Log("[MCQ] Quiz completed.");
            return;
        }

        LoadQuestion();
    }
}
