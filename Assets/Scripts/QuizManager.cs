
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers;
        public int correctAnswer;
    }

    [Header("Panels")]
    public GameObject welcomePanel;
    public GameObject questionPanel;
    public GameObject resultPanel;

    [Header("Question UI")]
    public TMP_Text questionNumberText;
    public TMP_Text questionText;
    public Button[] answerButtons;
    public TMP_Text[] answerButtonTexts;
    public TMP_Text feedbackText;
    public Button nextButton;
    public Slider progressBar;

    [Header("Result UI")]
    public TMP_Text scoreText;
    public TMP_Text percentageText;
    public TMP_Text messageText;
    public GameObject bibiResult;

    [Header("Questions")]
    public Question[] questions;

    private int currentQuestion = 0;
    private int score = 0;
    private bool answered = false;

    void Start()
    {
        welcomePanel.SetActive(true);
        questionPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    public void StartQuiz()
    {
        currentQuestion = 0;
        score = 0;

        welcomePanel.SetActive(false);
        questionPanel.SetActive(true);
        resultPanel.SetActive(false);

        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        answered = false;

        Question q = questions[currentQuestion];

        questionNumberText.text =
            "QUESTION " + (currentQuestion + 1) + " / " + questions.Length;

        questionText.text = q.questionText;

        progressBar.value = currentQuestion + 1;

        feedbackText.text = "";
        nextButton.interactable = false;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].transition = Selectable.Transition.None;
            answerButtons[i].image.color = Color.white;
            answerButtons[i].interactable = true;
            answerButtonTexts[i].text = q.answers[i];
        }

        nextButton.GetComponentInChildren<TMP_Text>().text =
            currentQuestion == questions.Length - 1
            ? "VIEW RESULTS"
            : "NEXT QUESTION";
    }

    public void SelectAnswer(int selectedAnswer)
    {
        if (answered) return;

        answered = true;

        Question q = questions[currentQuestion];

        if (selectedAnswer == q.correctAnswer)
        {
            score++;
            feedbackText.text = "Correct! Well done!";

            answerButtons[selectedAnswer].image.color = Color.green;
        }
        else
        {
            feedbackText.text = "Incorrect! The correct answer is: "
                + q.answers[q.correctAnswer];

            answerButtons[selectedAnswer].image.color = Color.red;

            answerButtons[q.correctAnswer].image.color = Color.green;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }

        nextButton.interactable = true;
    }

    public void NextQuestion()
    {
        currentQuestion++;

        if (currentQuestion < questions.Length)
        {
            DisplayQuestion();
        }
        else
        {
            ShowResults();
        }
    }

    void ShowResults()
    {
        questionPanel.SetActive(false);
        resultPanel.SetActive(true);

        scoreText.text = "Your Score: " + score + "/" + questions.Length;

        int percentage = Mathf.RoundToInt(
            (float)score / questions.Length * 100);

        percentageText.text = percentage + "%";

        if (score >= 8)
{
    messageText.text = "Excellent! Amazing work!";
    bibiResult.SetActive(true);
}
else if (score >= 5)
{
    messageText.text = "Good job! Keep learning!";
    bibiResult.SetActive(false);
}
else
{
    messageText.text = "Don't give up! Try again!";
    bibiResult.SetActive(false);
}
    }

    public void RetryQuiz()
    {
        resultPanel.SetActive(false);
        questionPanel.SetActive(true);
        bibiResult.SetActive(false);

        currentQuestion = 0;
        score = 0;

        DisplayQuestion();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("SelectionScene");
    }
}
