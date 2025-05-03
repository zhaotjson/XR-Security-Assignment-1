using UnityEngine;

public class MainBackgroundSpaceship : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f; // Speed of rotation
    [SerializeField] private float rotationRange = 45f; // Range of rotation around the Z-axis
    private float defaultZRotation = 90f; // Default Z-axis rotation
    private float targetZRotation; // Target Z-axis rotation
    private bool rotatingClockwise = true; // Direction of rotation

    private void Start()
    {
        // Set the initial target rotation
        targetZRotation = defaultZRotation + rotationRange;
    }

    private void Update()
    {
        // Get the current Z rotation
        float currentZRotation = transform.eulerAngles.z;

        // Determine the direction and target rotation
        if (rotatingClockwise && currentZRotation >= targetZRotation)
        {
            rotatingClockwise = false;
            targetZRotation = defaultZRotation - rotationRange;
        }
        else if (!rotatingClockwise && currentZRotation <= targetZRotation)
        {
            rotatingClockwise = true;
            targetZRotation = defaultZRotation + rotationRange;
        }

        // Smoothly rotate toward the target rotation
        float step = rotationSpeed * Time.deltaTime;
        float newZRotation = Mathf.MoveTowardsAngle(currentZRotation, targetZRotation, step);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, newZRotation);
    }
}
