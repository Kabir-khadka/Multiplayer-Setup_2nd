using UnityEngine;
using Unity.Netcode;

public class PipeRotation : NetworkBehaviour
{
    public float rotationSpeed = 50f; // Speed of rotation in degrees per second
    private NetworkVariable<float> networkRotation = new NetworkVariable<float>();
    private float currentRotation;

    void Start()
    {
        // Initialize the network variable with the current rotation for the server
        if (IsServer)
        {
            networkRotation.Value = transform.rotation.eulerAngles.z;
        }
    }

    void Update()
    {
        // Rotate the pipe only on the server
        if (IsServer)
        {
            RotatePipe();
        }

        // Smoothly update the rotation on the client side based on the network value
        if (IsClient)
        {
            SmoothRotate();
        }
    }

    // Rotate the pipe around the Z-axis (only on the server)
    private void RotatePipe()
    {
        // Calculate the new rotation and update the network variable
        float newRotation = networkRotation.Value + rotationSpeed * Time.deltaTime;

        // Update the network variable
        networkRotation.Value = newRotation;

        // Apply the rotation to the pipe on the server, with the Y-axis fixed to -90
        transform.rotation = Quaternion.Euler(0, -90, newRotation);

        // Store the current rotation for force calculation
        currentRotation = newRotation;
    }

    // Smoothly interpolate the rotation on the client side based on the networked value
    private void SmoothRotate()
    {
        // Interpolate between the current rotation and the networked rotation
        float smoothRotation = Mathf.LerpAngle(transform.rotation.eulerAngles.z, networkRotation.Value, Time.deltaTime * 10f);

        // Apply the smoothly interpolated rotation to the pipe, with the Y-axis fixed to -90
        transform.rotation = Quaternion.Euler(0, -90, smoothRotation);
    }

    // When the player is in contact with the pipe, apply a force to move them based on the rotation speed
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the player's Rigidbody component
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Apply a force to move the player based on the pipe's rotation speed and direction
                ApplyForceToPlayer(playerRb);
            }
        }
    }

    // Apply a force to the player based on the pipe's rotation direction and speed
    private void ApplyForceToPlayer(Rigidbody playerRb)
    {
        // Calculate the force direction based on the rotation (rotate around the Z-axis)
        Vector3 forceDirection = transform.right; // Apply force in the direction the pipe is rotating

        // The magnitude of the force depends on the rotation speed and the pipe's rotation direction
        float forceMagnitude = rotationSpeed * 0.1f; // You can adjust the multiplier for desired force

        // Apply the force to the player
        playerRb.AddForce(forceDirection * forceMagnitude, ForceMode.Force);
    }
}
