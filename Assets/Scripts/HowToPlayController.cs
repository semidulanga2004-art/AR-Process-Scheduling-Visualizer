using UnityEngine;

public class HowToPlayController : MonoBehaviour
{
    public GameObject HowToPlayPanel;
    public AudioSource audioSource;

    public void OpenPanel()
    {
        HowToPlayPanel.SetActive(true);
        audioSource.Play();
    }

    public void ClosePanel()
    {
        HowToPlayPanel.SetActive(false);
    }
}