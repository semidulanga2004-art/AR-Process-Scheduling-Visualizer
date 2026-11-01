
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    // Stores the selected scheduling algorithm
    public static string selectedAlgorithm = "FCFS";

    public void GoToSelection()
    {
        SceneManager.LoadScene("SelectionScene");
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("HomeScene");
    }

    // Select an algorithm and open the AR scene
    public void SelectFCFS()
    {
        selectedAlgorithm = "FCFS";
        SceneManager.LoadScene("ARLearningScene");
    }

    public void SelectSJF()
    {
        selectedAlgorithm = "SJF";
        SceneManager.LoadScene("ARLearningScene");
    }

    public void SelectPriority()
    {
        selectedAlgorithm = "Priority";
        SceneManager.LoadScene("ARLearningScene");
    }

    public void SelectRoundRobin()
    {
        selectedAlgorithm = "RoundRobin";
        SceneManager.LoadScene("ARLearningScene");
    }
    
    public void SelectSRTF()
    {
        selectedAlgorithm = "SRTF";
        SceneManager.LoadScene("ARLearningScene");
    }

        public void GoToQuiz()
    {
        SceneManager.LoadScene("QuizScene");
    }
}
