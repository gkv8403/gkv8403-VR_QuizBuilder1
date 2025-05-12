using UnityEngine;

[CreateAssetMenu(fileName = "QuestionDatabase", menuName = "Quiz/QuestionDatabase")]
public class QuestionDatabase : ScriptableObject
{
    public Question[] questions;  // List of all quiz questions
}
