using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class ScoreUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI component

    private void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("Score Text is not assigned. Assign it in the Inspector.");
        }
        else
        {
            UpdateScore(0); // Initialize the score display
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {score}";
        }
    }
}
