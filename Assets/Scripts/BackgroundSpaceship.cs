using UnityEngine;

public class BackgroundSpaceship : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f; // Speed of the spaceship
    [SerializeField] private Vector3 moveDirection = Vector3.right; // Direction of movement

    private void Update()
    {
        // Move the spaceship in the specified direction
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Reset position if it moves too far
        if (transform.position.x > 50f || transform.position.x < -50f)
        {
            transform.position = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
        }
    }
}
