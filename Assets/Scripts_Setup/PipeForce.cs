using Unity.Netcode;
using UnityEngine;

public class PipeForce : NetworkBehaviour
{
    public float impactForce = 5f; // Force applied to the player when interacting with the pipe

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pipe")) // Check if colliding with the pipe
        {
            if (IsServer) // Ensure force is applied on the server
            {
                HandlePipeCollision(collision);
            }
        }
    }

    private void HandlePipeCollision(Collision collision)
    {
        // Get the direction of the pipe's rotation (assuming it rotates around the Z-axis)
        Vector3 pipeRotation = collision.transform.forward; // Adjust based on the rotation direction

        // Apply force or any other reaction to the player based on pipe's rotation
        Vector3 forceDirection = pipeRotation.normalized * impactForce;

        // Only apply force if the Rigidbody exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force on the server side
            rb.AddForce(forceDirection, ForceMode.Impulse);

            // Optionally, you can also send an RPC to inform the clients, but typically physics should stay server-side
        }
    }
}
