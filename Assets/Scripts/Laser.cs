using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 10f; // Speed of the laser
    public float lifetime = 2f; // Time before the laser is destroyed

    private void Start()
    {
        // Adjust the laser's local scale to visually align it without affecting its movement
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.z, transform.localScale.y);

        // Destroy the laser after its lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the laser forward based on its local forward direction
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
