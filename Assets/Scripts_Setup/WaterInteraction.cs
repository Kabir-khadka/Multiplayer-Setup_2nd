using UnityEngine;
using Unity.Netcode;

public class WaterInteraction : NetworkBehaviour
{
    [Header("Buoyancy Settings")]
    public float buoyancyFactor = 2.0f;
    public float maxDepth = 5.0f;

    [Header("Drag Settings")]
    public float waterDrag = 3.0f;
    public float waterAngularDrag = 2.0f;
    public float normalDrag = 0.0f;
    public float normalAngularDrag = 0.05f;

    [Header("Bobbing Settings")]
    public float bobbingSpeed = 2.0f;
    public float bobbingAmplitude = 0.5f;

    private bool isInWater = false;
    private float waterSurfaceY;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        // Determine the water surface level from the collider's position
        waterSurfaceY = transform.position.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner && other.CompareTag("Player"))
        {
            Debug.Log("Heyyyyyyyy");
            playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                SetWaterDrag(playerRigidbody, waterDrag, waterAngularDrag);
            }
            isInWater = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsOwner && other.CompareTag("Player"))
        {
            if (playerRigidbody != null)
            {
                // Calculate depth below water surface
                float depth = Mathf.Clamp(waterSurfaceY - other.transform.position.y, 0, maxDepth);

                // Apply buoyancy force proportional to depth
                Vector3 buoyancyForce = new Vector3(0, depth * buoyancyFactor, 0);
                playerRigidbody.AddForce(buoyancyForce, ForceMode.Acceleration);

                // Add bobbing effect
                Vector3 bobbing = new Vector3(0, Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude, 0);
                playerRigidbody.AddForce(bobbing, ForceMode.Acceleration);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsOwner && other.CompareTag("Player"))
        {
            if (playerRigidbody != null)
            {
                SetWaterDrag(playerRigidbody, normalDrag, normalAngularDrag);
            }
            isInWater = false;
        }
    }

    private void SetWaterDrag(Rigidbody rb, float drag, float angularDrag)
    {
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
    }
}
