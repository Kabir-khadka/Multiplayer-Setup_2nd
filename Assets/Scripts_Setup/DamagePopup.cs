using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class DamagePopup : NetworkBehaviour
{
    [SerializeField] private Transform pfDamagePopup;
    private TextMeshPro textMesh;


    //ServerRpc to spawn the damage popup on all clients
    [ServerRpc(RequireOwnership = false)]
    public void SpawnDamagePopupServerRpc(Vector3 position, int damageAmount)
    {
        //Only the server spawns the damage popup
        DamagePopup.CreateAndNetworkSpawn(pfDamagePopup, position, damageAmount);
    }

    //Creates and spawns the damage popup across the network
    public static void CreateAndNetworkSpawn(Transform pfDamagePopup, Vector3 position, int damageAmount)
    {
        // Instantiate the popup at the desired position
        Transform damagePopupTransform = Instantiate(pfDamagePopup, position, Quaternion.identity);

        // Set up the damage text
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount);

        // Spawn the popup on the network
        NetworkObject networkObject = damagePopupTransform.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
    }

    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount)
    {
        textMesh.SetText(damageAmount.ToString());
    }

    private void Update()
    {
        float moveYSpeed = 2f;
        transform.position += new Vector3(0, moveYSpeed * Time.deltaTime, 0) ;
        // Optional: Add fading logic or destroy the object after a delay
        Destroy(gameObject, 2f); // Automatically destroys after 2 seconds
    }
}
