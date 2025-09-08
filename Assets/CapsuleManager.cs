using UnityEngine;

public class MoveCapsule : MonoBehaviour
{
    public float amplitude = 2.0f; // How far the capsule moves left/right
    public float frequency = 1.0f; // How fast the oscillation happens

    private Vector3 startPosition;

    void Start()
    {
        // Store the capsule's starting position
        startPosition = transform.position;
    }

    void Update()
    {
        // Update X position using sine
        Vector3 newPosition = startPosition;
        newPosition.x += Mathf.Sin(Time.time * frequency) * amplitude;
        newPosition.y += 25.0f * Mathf.Cos(Time.time * frequency / 10.0f) * amplitude;
        transform.position = newPosition;
    }
}