using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ShipFollowCamera : MonoBehaviour
{
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private float followDistance = 0.2f;
    [SerializeField] private float verticalOffset = -0.5f;
    [SerializeField] private float smoothSpeed = 5f;

    [SerializeField] private GameObject laserPrefab; // Laser prefab to instantiate
    [SerializeField] private Transform laserSpawnPoint; // Spawn point for the laser
    [SerializeField] private float laserSpeed = 0.0001f; // Speed of the laser
    [SerializeField] private float laserLifetime = 2f; // Time before the laser disappears

    [SerializeField] private AudioSource laserAudioSource; // Audio source for laser sound
    [SerializeField] private AudioClip laserSound; // Laser sound clip

    // Add a threshold to ignore small movements
    [SerializeField] private float movementThreshold = 0.001f; // Minimum movement to trigger rotation
    [SerializeField] private float rotationThreshold = 0.001f;  // Minimum rotation change to trigger updates

    private Transform arCameraTransform;
    private Vector3 previousPosition; // To store the ship's position in the previous frame
    private Coroutine laserCoroutine; // To manage the laser shooting coroutine

    private void OnEnable()
    {
        arCameraManager.frameReceived += OnCameraFrameReceived;

        // Ensure laserSpawnPoint is assigned
        if (laserSpawnPoint == null)
        {
            Debug.LogWarning("Laser spawn point is not assigned. Creating a default spawn point.");
            GameObject spawnPoint = new GameObject("DefaultLaserSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(0, 0, 0.5f); // Adjust offset as needed
            spawnPoint.transform.localRotation = Quaternion.identity;
            laserSpawnPoint = spawnPoint.transform;
        }

        if (laserPrefab == null)
        {
            Debug.LogError("Laser prefab is not assigned. Please assign it in the Inspector.");
            return; // Prevent further execution if laserPrefab is missing
        }

        // Use ARCameraManager to assign the camera transform
        AssignARCameraTransform();

        if (arCameraTransform == null)
        {
            Debug.LogError("AR Camera Transform is not assigned. Ensure the ARCameraManager is properly set up.");
            return; // Prevent further execution if arCameraTransform is missing
        }

        laserCoroutine = StartCoroutine(ShootLaserEverySecond());
    }

    private void AssignARCameraTransform()
    {
        if (arCameraManager != null && arCameraManager.GetComponent<Camera>() != null)
        {
            arCameraTransform = arCameraManager.GetComponent<Camera>().transform;
            Debug.Log("AR Camera Transform assigned successfully using ARCameraManager.");
        }
        else
        {
            Debug.LogError("ARCameraManager does not have a valid Camera component.");
        }
    }

    private void OnDisable()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
        if (laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // Ensure the AR camera transform is assigned if not already set
        if (arCameraTransform == null)
        {
            AssignARCameraTransform();
        }
    }

    private IEnumerator ShootLaserEverySecond()
    {
        while (true)
        {
            ShootLaser();
            yield return new WaitForSeconds(0.5f); // Wait for 1 second
        }
    }

    private void ShootLaser()
    {
        if (laserPrefab == null || laserSpawnPoint == null)
        {
            Debug.LogWarning("Laser prefab or spawn point is not assigned.");
            return;
        }

        // Play the laser sound
        if (laserAudioSource != null && laserSound != null)
        {
            laserAudioSource.PlayOneShot(laserSound);
            Debug.Log($"Laser sound played. Volume: {laserAudioSource.volume}, Spatial Blend: {laserAudioSource.spatialBlend}");
        }
        else
        {
            Debug.LogWarning("Laser audio source or sound clip is not assigned.");
        }

        // Calculate relative positions for the left and right lasers
        Vector3 leftSpawnPosition = laserSpawnPoint.TransformPoint(new Vector3(0, -0.0223f, 0)); // Left of the spawn point
        Vector3 rightSpawnPosition = laserSpawnPoint.TransformPoint(new Vector3(0, 0.0223f, 0)); // Right of the spawn point

        // Instantiate the left laser
        GameObject leftLaser = Instantiate(laserPrefab, leftSpawnPosition, laserSpawnPoint.rotation);
        Debug.Log($"Left laser instantiated at position: {leftSpawnPosition}");

        // Instantiate the right laser
        GameObject rightLaser = Instantiate(laserPrefab, rightSpawnPosition, laserSpawnPoint.rotation);
        Debug.Log($"Right laser instantiated at position: {rightSpawnPosition}");

        // Ensure the Laser script is attached to the left laser
        Laser leftLaserScript = leftLaser.GetComponent<Laser>();
        if (leftLaserScript != null)
        {
            leftLaserScript.speed = laserSpeed;
            leftLaserScript.lifetime = laserLifetime;
        }
        else
        {
            Debug.LogWarning("Left laser prefab does not have a Laser script attached.");
        }

        // Ensure the Laser script is attached to the right laser
        Laser rightLaserScript = rightLaser.GetComponent<Laser>();
        if (rightLaserScript != null)
        {
            rightLaserScript.speed = laserSpeed;
            rightLaserScript.lifetime = laserLifetime;
        }
        else
        {
            Debug.LogWarning("Right laser prefab does not have a Laser script attached.");
        }
    }

    private IEnumerator MoveLaserManually(GameObject laser)
    {
        float elapsedTime = 0f;
        while (elapsedTime < laserLifetime)
        {
            laser.transform.position += laserSpawnPoint.forward * laserSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(laser);
    }

    private void Update()
    {
        if (arCameraTransform == null) return;

        // Calculate target position in front of the camera
        Vector3 targetPosition = arCameraTransform.position +
                                arCameraTransform.forward * followDistance +
                                Vector3.up * verticalOffset;

        // Smoothly move the ship to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Calculate movement direction
        Vector3 movementDirection = (transform.position - previousPosition).normalized;

        // Ensure the ship always faces the same direction as the camera with a 90-degree adjustment on the Z-axis
        Quaternion baseRotation = arCameraTransform.rotation * Quaternion.Euler(0, 0, 90);
        Quaternion targetRotation = baseRotation;

        if (movementDirection != Vector3.zero)
        {
            // Check if movement exceeds the threshold
            if ((transform.position - previousPosition).magnitude > movementThreshold)
            {
                // Roll on the Z-axis for left/right movement
                float rollZ = -movementDirection.x * 30f;

                // Roll on the X-axis for up/down movement
                float rollX = movementDirection.y * 30f;

                // Combine the rolls with the base rotation
                targetRotation *= Quaternion.Euler(rollX, 0, rollZ);
            }
        }

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);

        // Ensure the laser spawn point is always relative to the ship
        if (laserSpawnPoint != null)
        {
            Vector3 localOffset = new Vector3(0, 0, 0.05f); // Adjusted offset to spawn closer to the ship
            laserSpawnPoint.position = transform.TransformPoint(localOffset); // Convert local offset to world position
            laserSpawnPoint.rotation = transform.rotation; // Match the ship's rotation
        }

        // Update previous position
        previousPosition = transform.position;
    }
}