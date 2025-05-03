using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VultureDroid : MonoBehaviour
{
    private Transform playerTransform;
    [SerializeField] private float moveSpeed = 1f; // Speed at which the droid moves toward the player
    [SerializeField] private int points = 10; // Points awarded for destroying this droid
    private bool isDestroyed = false; // Prevent multiple triggers

    void Start()
    {
        // Ensure the default rotation is (0, 0, 90)
        transform.rotation = Quaternion.Euler(0, 180, 90);

        // Find the player in the scene
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found. Ensure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Face the player while maintaining the default (0, 0, 90) orientation
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation * Quaternion.Euler(0, 180, 90), Time.deltaTime * 5f);

            // Move toward the player
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple triggers
        if (isDestroyed) return;

        if (other.CompareTag("Laser"))
        {
            isDestroyed = true; // Mark as destroyed to prevent further triggers

            // Award points to the player
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.AddScore(points);
            }

            // Destroy the VultureDroid
            Destroy(gameObject);

            // Optionally, destroy the laser as well
            Destroy(other.gameObject);

            Debug.Log("VultureDroid destroyed by laser.");
        }
        else if (other.CompareTag("Player"))
        {
            isDestroyed = true; // Mark as destroyed to prevent further triggers

            // Destroy the player if the droid reaches them
            Destroy(other.gameObject);
            Debug.Log("Player destroyed by VultureDroid.");
        }
    }
}
