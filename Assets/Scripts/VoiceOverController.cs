using UnityEngine;

public class VoiceOverController : MonoBehaviour
{
    public AudioSource voiceOverAudio;

    public AudioClip fcfsVoice;
    public AudioClip sjfVoice;
    public AudioClip priorityVoice;
    public AudioClip roundRobinVoice;
    public AudioClip srtfVoice;

        public void StartVoiceOver()
        {
            PlaySelectedVoiceOver();
        }
    void PlaySelectedVoiceOver()
    {
        string selected = SceneNavigation.selectedAlgorithm;

        switch (selected)
        {
            case "FCFS":
                voiceOverAudio.clip = fcfsVoice;
                break;

            case "SJF":
                voiceOverAudio.clip = sjfVoice;
                break;

            case "Priority":
                voiceOverAudio.clip = priorityVoice;
                break;

            case "RoundRobin":
                voiceOverAudio.clip = roundRobinVoice;
                break;

            case "SRTF":
                voiceOverAudio.clip = srtfVoice;
                break;
        }

        if (voiceOverAudio.clip != null)
        {
            voiceOverAudio.Play();
        }
        else
        {
            Debug.LogWarning("Voice-over not assigned for: " + selected);
        }
    }
}