using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Button
using UnityEngine.Networking; // Required for web requests
using System.Collections; // Required for Coroutines
using System.Text; // Required for Encoding

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI component for displaying the score
    [SerializeField] private TMP_InputField playerNameInput; // Input field for player name
    [SerializeField] private Button submitButton; // Button to submit the score
    [SerializeField] private GameObject scoreInputSection; // Parent object for input field and button
    [SerializeField] private GameObject scoreSubmittedSection; // Object to show after submission (optional)

    private int finalScore; // Store the score locally
    private string highScoreAPIUrl = "http://localhost:3000/api/highscores"; // Define the API URL here

    // Simple class for JSON serialization
    [System.Serializable]
    private class HighScoreEntry
    {
        public string playerName;
        public int score;
    }

    private void Start()
    {
        // Retrieve the score from PlayerPrefs
        finalScore = PlayerPrefs.GetInt("FinalScore", 0);

        // Display the score
        if (scoreText != null)
        {
            scoreText.text = $"Your Score: {finalScore}";
        }
        else
        {
            Debug.LogError("Score Text is not assigned in the Inspector.");
        }

        // Ensure Player Name Input is assigned
        if (playerNameInput == null)
        {
             Debug.LogError("Player Name Input is not assigned in the Inspector.");
             if (submitButton != null) submitButton.interactable = false;
        }
        else // Only set initial UI state if input field exists
        {
             if (scoreInputSection != null) scoreInputSection.SetActive(true);
             if (scoreSubmittedSection != null) scoreSubmittedSection.SetActive(false);
             if (submitButton != null) submitButton.interactable = true; // Enable button initially
        }
    }

    // Method to be called by the Submit Button's OnClick event
    public void SubmitPlayerScore()
    {
        if (playerNameInput == null || submitButton == null)
        {
            Debug.LogError("Cannot submit score. Check Player Name Input and Submit Button Inspector assignments.");
            return;
        }

        string playerName = playerNameInput.text;

        // Use a default name if the input is empty
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }

        // Submit the score directly using a coroutine
        Debug.Log($"Submitting score directly: {playerName} - {finalScore}");
        StartCoroutine(SubmitScoreDirectlyCoroutine(playerName, finalScore));

        // Update UI: Disable input/button, show confirmation
        submitButton.interactable = false;
        playerNameInput.interactable = false; // Make input field non-editable

        if (scoreInputSection != null)
        {
            // Optionally hide the whole input section
            // scoreInputSection.SetActive(false);
        }
        if (scoreSubmittedSection != null)
        {
            scoreSubmittedSection.SetActive(true); // Show "Score Submitted!" message
        }
    }

    // New Coroutine to handle the web request directly
    private IEnumerator SubmitScoreDirectlyCoroutine(string playerName, int score)
    {
        HighScoreEntry newScore = new HighScoreEntry { playerName = playerName, score = score };
        string json = JsonUtility.ToJson(newScore);

        Debug.Log($"[SubmitScoreDirectly] Attempting to POST to: {highScoreAPIUrl}");
        Debug.Log($"[SubmitScoreDirectly] JSON Payload: {json}");

        // Using UnityWebRequest for POST
        using (UnityWebRequest request = new UnityWebRequest(highScoreAPIUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Optional: Add timeout
            request.timeout = 10; // Timeout after 10 seconds

            yield return request.SendWebRequest();

            Debug.Log($"[SubmitScoreDirectly] Request completed. Result: {request.result}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[SubmitScoreDirectly] Success! Response Code: {request.responseCode}");
                // Optionally process response: Debug.Log(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[SubmitScoreDirectly] Failed. Error: {request.error}");
                Debug.LogError($"[SubmitScoreDirectly] Response Code: {request.responseCode}");
                if (request.downloadHandler != null)
                {
                    Debug.LogError($"[SubmitScoreDirectly] Response Body (Error): {request.downloadHandler.text}");
                }
                // Optional: Re-enable button or show error message to user here
                // submitButton.interactable = true;
                // playerNameInput.interactable = true;
                // if (scoreSubmittedSection != null) scoreSubmittedSection.SetActive(false);
                // // Add a specific error message display?
            }
        } // The 'using' statement ensures request.Dispose() is called.
    }

    public void RestartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("Intro");
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }
}
