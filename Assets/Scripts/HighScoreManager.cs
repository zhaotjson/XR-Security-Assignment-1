using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HighScoreManager : MonoBehaviour
{
    [SerializeField] private GameObject highScoreCanvas; // Canvas to display high scores
    [SerializeField] private TextMeshProUGUI highScoreText; // Text element to display scores
    private string highScoreAPIUrl = "http://localhost:3000/api/highscores"; // Verify this URL

    private void Start()
    {
        // Log the URL being used at runtime
        Debug.Log($"[HighScoreManager] Initialized. API URL: {highScoreAPIUrl}");

        // Ensure the high score canvas is hidden initially
        if (highScoreCanvas != null)
        {
            highScoreCanvas.SetActive(false);
        }
    }

    public void ShowHighScores()
    {
        // Show the high score canvas
        if (highScoreCanvas != null)
        {
            highScoreCanvas.SetActive(true);
        }

        // Fetch and display high scores
        StartCoroutine(FetchHighScores(highScores =>
        {
            DisplayHighScores(highScores);
        }));
    }

    // New method to hide the high score canvas
    public void HideHighScores()
    {
        if (highScoreCanvas != null)
        {
            highScoreCanvas.SetActive(false);
        }
    }

    public IEnumerator FetchHighScores(System.Action<List<HighScoreEntry>> callback)
    {
        Debug.Log($"[FetchHighScores] Attempting to fetch from: {highScoreAPIUrl}");
        UnityWebRequest request = UnityWebRequest.Get(highScoreAPIUrl);

        // Optional: Add timeout
        request.timeout = 10; // Timeout after 10 seconds

        yield return request.SendWebRequest();

        Debug.Log($"[FetchHighScores] Request completed. Result: {request.result}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[FetchHighScores] Success! Response Code: {request.responseCode}");
            Debug.Log($"[FetchHighScores] Response Body: {request.downloadHandler.text}");
            try
            {
                HighScoreList highScores = JsonUtility.FromJson<HighScoreList>(request.downloadHandler.text);
                if (highScores != null && highScores.highScores != null)
                {
                    callback?.Invoke(highScores.highScores);
                }
                else
                {
                    Debug.LogError("[FetchHighScores] Failed to parse JSON response or highScores list is null.");
                    if (highScoreText != null) highScoreText.text = "Error parsing scores.";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FetchHighScores] JSON Parsing Exception: {ex.Message}");
                if (highScoreText != null) highScoreText.text = "Error parsing scores.";
            }
        }
        else
        {
            Debug.LogError($"[FetchHighScores] Failed. Error: {request.error}");
            Debug.LogError($"[FetchHighScores] Response Code: {request.responseCode}");
            if (request.downloadHandler != null)
            {
                Debug.LogError($"[FetchHighScores] Response Body (Error): {request.downloadHandler.text}");
            }
            if (highScoreText != null)
            {
                highScoreText.text = $"Failed: {request.error}";
            }
        }
    }

    public void SubmitHighScore(string playerName, int score)
    {
        StartCoroutine(SubmitHighScoreCoroutine(playerName, score));
    }

    private IEnumerator SubmitHighScoreCoroutine(string playerName, int score)
    {
        HighScoreEntry newScore = new HighScoreEntry { playerName = playerName, score = score };
        string json = JsonUtility.ToJson(newScore);

        Debug.Log($"[SubmitHighScore] Attempting to POST to: {highScoreAPIUrl}");
        Debug.Log($"[SubmitHighScore] JSON Payload: {json}");

        UnityWebRequest request = new UnityWebRequest(highScoreAPIUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();

        // Optional: Add timeout
        request.timeout = 10; // Timeout after 10 seconds

        yield return request.SendWebRequest();

        Debug.Log($"[SubmitHighScore] Request completed. Result: {request.result}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[SubmitHighScore] Success! Response Code: {request.responseCode}");
            Debug.Log($"[SubmitHighScore] Response Body: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[SubmitHighScore] Failed. Error: {request.error}");
            Debug.LogError($"[SubmitHighScore] Response Code: {request.responseCode}");
            if (request.downloadHandler != null)
            {
                Debug.LogError($"[SubmitHighScore] Response Body (Error): {request.downloadHandler.text}");
            }
        }
    }

    private void DisplayHighScores(List<HighScoreEntry> highScores)
    {
        if (highScoreText == null) return;

        // Define column widths (adjust as needed)
        int rankWidth = 4; // Might need adjustment if ranks go to 10 (e.g., "10.")
        int nameWidth = 15;
        // Score width is less critical to pad, but ensure header matches

        // Build the header with padding
        string header = "Rank".PadRight(rankWidth) + " | " + "Player Name".PadRight(nameWidth) + " | Score\n";
        string separator = new string('-', header.Length - 1) + "\n"; // Dynamic separator length

        highScoreText.text = "High Scores:\n";
        highScoreText.text += separator;
        highScoreText.text += header;
        highScoreText.text += separator;

        // Build each score line with padding
        // This loop already handles any number of scores received
        for (int i = 0; i < highScores.Count; i++)
        {
            string rankStr = (i + 1).ToString() + ".";
            string nameStr = highScores[i].playerName;
            // Truncate long names if necessary, or adjust nameWidth
            if (nameStr.Length > nameWidth)
            {
                nameStr = nameStr.Substring(0, nameWidth - 3) + "...";
            }

            highScoreText.text += rankStr.PadRight(rankWidth) + " | " + nameStr.PadRight(nameWidth) + " | " + highScores[i].score + "\n";
        }
    }

    [System.Serializable]
    public class HighScoreEntry
    {
        public string playerName;
        public int score;
    }

    [System.Serializable]
    public class HighScoreList
    {
        public List<HighScoreEntry> highScores;
    }
}
