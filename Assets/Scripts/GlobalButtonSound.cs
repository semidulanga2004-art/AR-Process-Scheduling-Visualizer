using UnityEngine;
using UnityEngine.UI;

public class GlobalButtonSound : MonoBehaviour
{
    public AudioSource audioSource;
    public Button buttonToExclude;

    private void Start()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

       foreach (Button button in buttons)
{
    if (button == buttonToExclude)
        continue;

    button.onClick.AddListener(PlayClickSound);
}
    }

    public void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}