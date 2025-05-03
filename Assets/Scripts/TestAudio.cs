using UnityEngine;

public class TestAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip testClip;

    void Start()
    {
        if (audioSource != null && testClip != null)
        {
            audioSource.PlayOneShot(testClip);
            Debug.Log("Test sound played.");
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is not assigned.");
        }
    }
}
