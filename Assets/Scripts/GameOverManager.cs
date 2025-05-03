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
    // !! IMPORTANT: Replace <Your-Computer-IP> with your actual local network IP address !!
    private string highScoreAPIUrl = "https://ca5b-131-179-134-4.ngrok-free.app/api/highscores"; // Manually set API URL

    // Simple class for JSON serialization
    [System.Serializable]
    private class HighScoreEntry
    {
        public string playerName;
        public int score;
    }

    private void Start()
    {
        // --- Validate URL ---
        Debug.Log($"[GameOverManager] Using API URL: {highScoreAPIUrl}");
        bool isUrlValid = !string.IsNullOrEmpty(highScoreAPIUrl) && !highScoreAPIUrl.Contains("<Your-Computer-IP>");

        if (!isUrlValid)
        {
            Debug.LogError("[GameOverManager] API URL is invalid or still contains placeholder <Your-Computer-IP>. Please edit GameOverManager.cs and set the correct URL.");
            // Disable submission if URL is invalid
            if (submitButton != null) submitButton.interactable = false;
            if (scoreInputSection != null) scoreInputSection.SetActive(false);
        }
        // --- End Validation ---

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
        // Enable UI only if URL and input are valid
        else if (isUrlValid)
        {
            if (scoreInputSection != null) scoreInputSection.SetActive(true);
            if (scoreSubmittedSection != null) scoreSubmittedSection.SetActive(false);
            if (submitButton != null) submitButton.interactable = true; // Enable button initially
        }
        else // Keep UI disabled otherwise
        {
            if (scoreInputSection != null) scoreInputSection.SetActive(false);
            if (scoreSubmittedSection != null) scoreSubmittedSection.SetActive(false);
            if (submitButton != null) submitButton.interactable = false;
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
        // Add check for valid URL before submitting
        if (string.IsNullOrEmpty(highScoreAPIUrl) || highScoreAPIUrl.Contains("<Your-Computer-IP>"))
        {
             Debug.LogError("Cannot submit score. API URL is not configured correctly in GameOverManager.cs.");
             return;
        }

        string playerName = playerNameInput.text;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }

        Debug.Log($"Submitting score directly: {playerName} - {finalScore}");
        StartCoroutine(SubmitScoreDirectlyCoroutine(playerName, finalScore));

        submitButton.interactable = false;
        playerNameInput.interactable = false;
        if (scoreInputSection != null) { /* ... */ }
        if (scoreSubmittedSection != null) { scoreSubmittedSection.SetActive(true); }
    }

    // Coroutine to handle the web request directly
    private IEnumerator SubmitScoreDirectlyCoroutine(string playerName, int score)
    {
        // Ensure URL is valid before proceeding
        if (string.IsNullOrEmpty(highScoreAPIUrl) || highScoreAPIUrl.Contains("<Your-Computer-IP>"))
        {
            Debug.LogError("[SubmitScoreDirectly] Invalid API URL. Cannot send request.");
            yield break; // Stop the coroutine
        }

        string fullApiUrl = highScoreAPIUrl;

        HighScoreEntry newScore = new HighScoreEntry { playerName = playerName, score = score };
        string json = JsonUtility.ToJson(newScore);

        Debug.Log($"[SubmitScoreDirectly] Attempting to POST to: {fullApiUrl}");
        Debug.Log($"[SubmitScoreDirectly] JSON Payload: {json}");

        using (UnityWebRequest request = new UnityWebRequest(fullApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            Debug.Log($"[SubmitScoreDirectly] Request completed. Result: {request.result}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[SubmitScoreDirectly] Success! Response Code: {request.responseCode}");
            }
            else
            {
                Debug.LogError($"[SubmitScoreDirectly] Failed. Error: {request.error}");
                Debug.LogError($"[SubmitScoreDirectly] Response Code: {request.responseCode}");
                if (request.downloadHandler != null)
                {
                    Debug.LogError($"[SubmitScoreDirectly] Response Body (Error): {request.downloadHandler.text}");
                }
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Intro");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
