using QFSW.QC;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Threading.Tasks;

public class TestRelay : MonoBehaviour
{

    public static TestRelay Instance { get; private set; }

    //Function that signs in players anonymously.
    private async void Start()
    {
      /*  await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();*/
    }

    private void Awake()
    {
        Instance = this;
    }

    [Command]
    public async Task<string> CreateRelay()
    {
        try
        {

            //creates a new relay allocation for multiplayer purpose
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            //Get the code. The code is the allocationId of the allocation object variable created above.
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //Print the joincode
            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;

        } catch(RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    [Command]
    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " + joinCode);
            JoinAllocation joinAllocation  =   await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
