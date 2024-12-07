using UnityEngine;
using Unity.Netcode;

public class Testing : MonoBehaviour
{
    [SerializeField] private Transform pfDamagePopup;
    [SerializeField] private LayerMask raycastLayerMask; // Layer mask for raycasting
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Check if the ray hits something
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, raycastLayerMask))
            {
                // Check if the object hit by the ray has the tag "Player"
                if (hitInfo.collider.CompareTag("Player"))
                {

                    // Use the position of the player object (hit collider's transform position)
                    Vector3 spawnPosition = hitInfo.collider.transform.position;

                    

                    // Call the ServerRpc to spawn the damage popup
                    int damageAmount = Random.Range(50, 200); // Random damage for testing
                    SpawnDamagePopupOnServer(spawnPosition, damageAmount);
                }
            }
        }
    }

    // Calls the ServerRpc to spawn the damage popup
    [ServerRpc(RequireOwnership = false)]
    private void SpawnDamagePopupOnServer(Vector3 position, int damageAmount)
    {
        // Call the method to spawn the damage popup across the network
        DamagePopup.CreateAndNetworkSpawn(pfDamagePopup, position, damageAmount);
    }
}
