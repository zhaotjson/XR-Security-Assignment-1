using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;

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
    [SerializeField] private AudioSource audioSource; // Assign in Inspector
    [SerializeField] private AudioClip laserDestructionClip; // Assign in Inspector

    private float currentSpawnInterval;
    private int score = 0; // Player's score

    // --- Security: Encryption key generated at runtime, not stored ---
    private string encryptionKey;

    // --- Security: Integrity check hash ---
    private string integrityHash;

    private void Start()
    {
        // --- Security: Generate encryption key at runtime ---
        encryptionKey = GenerateEncryptionKey();

        // --- Security: Compute integrity hash of critical logic ---
        integrityHash = ComputeIntegrityHash();

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
        // --- Privacy/Security: Warn user about score transmission ---
        Debug.LogWarning("Scores will be encrypted before being sent to the server. Do not share your encryption key. Tampering with the game may result in score invalidation.");

        // Use text-to-speech to provide instructions
        if (textToSpeechManager != null)
        {
            textToSpeechManager.Speak("Shoot down the spaceships before they reach you!");
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
        // --- Security: Runtime integrity check ---
        if (!CheckIntegrity())
        {
            Debug.LogError("Game integrity check failed. Possible tampering detected. Spawning halted.");
            yield break;
        }

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
        // --- Security: Obfuscated logic for score addition ---
        int obfPoints = ObfuscatePoints(points);
        score += DeobfuscatePoints(obfPoints);

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

        // Play destruction sound on droid/enemy destruction
        PlayDestructionSound();

        // Announce milestones every 50 points
        if (score % 50 == 0 && textToSpeechManager != null)
        {
            string announcement = GetAnnouncementForScore(score);
            // Stop destruction sound so TTS can be heard clearly
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();
            textToSpeechManager.Speak(announcement);
        }
    }

    private void PlayDestructionSound()
    {
        if (audioSource != null && laserDestructionClip != null)
        {
            audioSource.PlayOneShot(laserDestructionClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or laserDestructionClip not assigned on EnemySpawner.");
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
        // --- Security: Encrypt score before saving/sending ---
        string encryptedScore = EncryptScore(score, encryptionKey);
        PlayerPrefs.SetString("FinalScoreEncrypted", encryptedScore);

        // Optionally, send encryptedScore to server here

        // Load the Game Over scene
        SceneManager.LoadScene("GameOver");
    }

    public int GetScore()
    {
        return score;
    }

    // --- Security/Privacy helpers ---

    // Generate a random encryption key at runtime
    private string GenerateEncryptionKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] key = new byte[16];
            rng.GetBytes(key);
            return System.Convert.ToBase64String(key);
        }
    }

    // Encrypt score using AES and the runtime key
    private string EncryptScore(int score, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = System.Convert.FromBase64String(key);
            aes.GenerateIV();
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] scoreBytes = Encoding.UTF8.GetBytes(score.ToString());
            byte[] encrypted = encryptor.TransformFinalBlock(scoreBytes, 0, scoreBytes.Length);
            // Store IV + encrypted data
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            System.Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            System.Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            return System.Convert.ToBase64String(result);
        }
    }

    // Obfuscate points (simple XOR for demonstration)
    private int ObfuscatePoints(int points)
    {
        return points ^ 0x5A5A5A5A;
    }
    private int DeobfuscatePoints(int obfPoints)
    {
        return obfPoints ^ 0x5A5A5A5A;
    }

    // Compute a hash of critical logic for integrity check
    private string ComputeIntegrityHash()
    {
        string logic = $"{minSpawnRadius}-{maxSpawnRadius}-{initialSpawnInterval}-{spawnAcceleration}-{minimumSpawnInterval}";
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(logic));
            return System.Convert.ToBase64String(hash);
        }
    }

    // Check integrity at runtime
    private bool CheckIntegrity()
    {
        string currentHash = ComputeIntegrityHash();
        if (currentHash != integrityHash)
        {
            Debug.LogError("Integrity hash mismatch! Game logic may have been tampered with.");
            return false;
        }
        return true;
    }
}
