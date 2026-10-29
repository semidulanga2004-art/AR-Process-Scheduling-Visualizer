
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void GoToSelection()
    {
        SceneManager.LoadScene("SelectionScene");
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("HomeScene");
    }
    // Open the AR Learning Scene
    public void GoToARLearning()
    {
        SceneManager.LoadScene("ARLearningScene");
    }
}
