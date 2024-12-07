using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class HitDetector : NetworkBehaviour
{
    /*private bool hitProcessed = false; // Prevent double processing*/

    private HashSet<Collider> processedColliders = new HashSet<Collider>();
    private float hitCooldown = 0.7f; // Cooldown duration in seconds
    private Dictionary<Collider, float> lastHitTime = new Dictionary<Collider, float>();

    [SerializeField] private Transform damagePopupPrefab;

    private void OnTriggerEnter(Collider other)
    {

        if (processedColliders.Contains(other)) return;//hitProcessed ||
        
        //processedColliders.Add(other);
/*
        if (hitProcessed) return; // Prevent duplicate triggers
        hitProcessed = true;*/

        // Check if the collider is the TorsoCollider
        if (other.CompareTag("TorsoCollider"))
        {


            float currentTime = Time.time;
            if (lastHitTime.TryGetValue(other, out float lastTime))
            {
                if (currentTime - lastTime < hitCooldown) return; // Ignore if within cooldown
            }

            // Update the last hit time
            lastHitTime[other] = currentTime;

            // Add the collider to processed set
            processedColliders.Add(other);


            // Ensure the collider has a Rigidbody and a HitDetector attached
            Rigidbody rb = GetComponent<Rigidbody>();
            HitDetector hitDetector = GetComponent<HitDetector>();

            if (rb != null && hitDetector != null)
            {
                Debug.Log($"Hit detected by: {gameObject.name} on: {other.name}");
                // Trigger the hit logic
                NetworkObject targetNetworkObject = other.GetComponentInParent<NetworkObject>();
                if (targetNetworkObject != null)
                {
                    ulong targetNetworkObjectId = targetNetworkObject.NetworkObjectId;
                    TriggerHitServerRpc(targetNetworkObjectId, Random.Range(10, 50)); // Random damage value
                }
            }
        }

        // Reset hit processing after a short delay
        Invoke(nameof(ResetHitProcessed), 0.1f);
    }

    private void ResetHitProcessed()
    {
        //hitProcessed = false;
        processedColliders.Clear(); // Clear the processed set
    }

  /*  // Function to handle triggering the animation
    private void TriggerHitAnimation(Collider other)
    {
        NetworkObject targetNetworkObject = other.GetComponentInParent<NetworkObject>();
        if (targetNetworkObject == null)
        {
            Debug.LogWarning("TorsoCollider does not have a NetworkObject parent!");
            return;
        }

        // Trigger the animation on the server side
        TriggerHitAnimationServerRpc(targetNetworkObject.NetworkObjectId);
    }*/

    [ServerRpc(RequireOwnership = false)]
    private void TriggerHitServerRpc(ulong targetNetworkObjectId, int damageAmount)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var targetNetworkObject))
        {
            // Trigger animation on the target player
            Animator targetAnimator = targetNetworkObject.GetComponentInChildren<Animator>();
            if (targetAnimator != null)
            {
                targetAnimator.ResetTrigger("HitTorso");
                targetAnimator.SetTrigger("HitTorso");
            }

            // Notify all clients to update the animation
            TriggerHitClientRpc(targetNetworkObjectId, damageAmount);
        }
    }

    [ClientRpc]
    private void TriggerHitClientRpc(ulong targetNetworkObjectId, int damageAmount)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var targetNetworkObject))
        {
            Animator targetAnimator = targetNetworkObject.GetComponentInChildren<Animator>();
            if (targetAnimator != null)
            {
                Debug.Log($"Triggering animation on client for: {targetNetworkObject.name}");
                targetAnimator.ResetTrigger("HitTorso");
                targetAnimator.SetTrigger("HitTorso");
            }

            // Spawn the damage popup on the hit player's position
            Transform hitPlayerTransform = targetNetworkObject.transform;
            DamagePopup.CreateAndNetworkSpawn(damagePopupPrefab, hitPlayerTransform.position, damageAmount);
        }
    }
}
