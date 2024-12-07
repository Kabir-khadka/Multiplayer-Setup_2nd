using QFSW.QC.Actions;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnObjectTransform;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 56,
            _bool = true,

        },NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public struct MyCustomData: INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

    private void Update()
    {
        

        //Creating the logic for separate movements
        if (!IsOwner) return;

        //Logic for spawn
        if (Input.GetKeyDown(KeyCode.T))
        {
            spawnObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            

            /*
            randomNumber.Value = new MyCustomData
            {
                _int = 10,
                _bool = false,
                message = "You shall not pass!",
            };*/

            //TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1} } });
        }


        //Logic for despawn
        if(Input.GetKeyDown(KeyCode.Y))
        {
            Destroy(spawnObjectTransform.gameObject);
        }

        //Creating direction in 3D
        Vector3 moveDir = new Vector3(0, 0, 0);

        //Assigning movement with keycode
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("TestClientRpc");
    }

}
