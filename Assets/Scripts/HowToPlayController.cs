using UnityEngine;

public class HowToPlayController : MonoBehaviour
{
    public GameObject HowToPlayPanel;

    public void OpenPanel()
    {
        HowToPlayPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        HowToPlayPanel.SetActive(false);
    }
}