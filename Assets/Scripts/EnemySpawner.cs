using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab; // Prefab of the enemy to spawn
    [SerializeField] private Transform playerTransform; // Reference to the player's transform
    [SerializeField] private float minSpawnRadius = 25f; // Minimum distance from the player
    [SerializeField] private float maxSpawnRadius = 50f; // Maximum distance from the player
    [SerializeField] private float initialSpawnInterval = 5f; // Initial time interval between spawns
    [SerializeField] private float spawnAcceleration = 0.1f; // Amount to reduce spawn interval after each spawn
    [SerializeField] private float minimumSpawnInterval = 1f; // Minimum spawn interval
    [SerializeField] private ScoreUIManager scoreUIManager; // Reference to the Score UI Manager
    [SerializeField] private TextToSpeechManager textToSpeechManager; // Reference to the Text-to-Speech Manager

    private float currentSpawnInterval;
    private int score = 0; // Player's score

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned. Assign it in the Inspector.");
            return;
        }

        currentSpawnInterval = initialSpawnInterval;

        // Start the grace period
        StartCoroutine(GracePeriod());
    }

    private IEnumerator GracePeriod()
    {
        // Use text-to-speech to provide instructions
        if (textToSpeechManager != null)
        {
            textToSpeechManager.Speak("Say shoot to start shooting, and stop to stop shooting.");
        }
        else
        {
            Debug.LogWarning("TextToSpeechManager is not assigned.");
        }

        // Wait for 3 seconds before starting enemy spawns
        yield return new WaitForSeconds(3f);

        // Start spawning enemies
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnInterval);

            // Gradually accelerate spawn rate
            currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnAcceleration, minimumSpawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned. Assign it in the Inspector.");
            return;
        }

        // Generate a random position around the player within the specified range
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0; // Keep the enemy on the same horizontal plane
        float spawnDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = playerTransform.position + randomDirection * spawnDistance;

        // Spawn the enemy facing the player
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");

        // Update the score on the UI
        if (scoreUIManager != null)
        {
            scoreUIManager.UpdateScore(score);
        }
        else
        {
            Debug.LogWarning("ScoreUIManager is not assigned.");
        }

        // Announce milestones every 50 points
        if (score % 50 == 0 && textToSpeechManager != null)
        {
            string announcement = GetAnnouncementForScore(score);
            textToSpeechManager.Speak(announcement);
        }
    }

    private string GetAnnouncementForScore(int score)
    {
        string[] words = { "HOT STREAK", "SPECTACULAR", "AMAZING", "UNSTOPPABLE", "INCREDIBLE", "PHENOMENAL", "LEGENDARY", "EPIC" };
        int randomIndex = Random.Range(0, words.Length);
        return $"{words[randomIndex]}, {score} points!";
    }

    public void GameOver()
    {
        // Save the score to PlayerPrefs
        PlayerPrefs.SetInt("FinalScore", score);

        // Load the Game Over scene
        SceneManager.LoadScene("GameOver");
    }

    public int GetScore()
    {
        return score;
    }
}
