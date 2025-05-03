using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ShipFollowCamera : MonoBehaviour
{
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private float followDistance = 0.2f;
    [SerializeField] private float verticalOffset = -0.5f;
    [SerializeField] private float smoothSpeed = 5f;

    private Transform arCameraTransform;
    private Vector3 previousPosition; // To store the ship's position in the previous frame

    private void OnEnable()
    {
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (arCameraTransform == null && arCameraManager.GetComponent<Camera>() != null)
        {
            arCameraTransform = arCameraManager.GetComponent<Camera>().transform;
        }
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

        // Align the ship's rotation with the camera's forward direction
        Quaternion baseRotation = Quaternion.LookRotation(arCameraTransform.forward, Vector3.up) * Quaternion.Euler(0, 0, 90);
        Quaternion targetRotation = baseRotation;

        if (movementDirection != Vector3.zero)
        {
            // Roll on the z-axis for left/right movement
            float rollZ = -movementDirection.x * 30f;

            // Roll on the x-axis for up/down movement
            float rollX = movementDirection.y * 30f;

            // Combine the rolls with the base rotation
            targetRotation *= Quaternion.Euler(rollX, 0, rollZ);
        }

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);

        // Update previous position
        previousPosition = transform.position;
    }
}