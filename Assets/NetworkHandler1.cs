using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkHandler : NetworkBehaviour
{
    public GameObject hostSphere;
    public GameObject clientSphere;

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started successfully.");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"Client {clientId} connected.");
        var clientNetObj = clientSphere.GetComponent<NetworkObject>();
        if (clientNetObj != null)
        {
            clientNetObj.ChangeOwnership(clientId);
        }
        else
        {
            Debug.LogError("Client sphere NetworkObject is null!");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"Client {clientId} disconnected.");
    }

    public void StartHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "0.0.0.0"; // Bind to all interfaces
        transport.ConnectionData.ServerListenAddress = "0.0.0.0";
        transport.ConnectionData.Port = 7777;
        bool started = NetworkManager.Singleton.StartHost();
        Debug.Log($"Host start attempt: {(started ? "Success" : "Failed")}");
        if (started)
        {
            Debug.Log($"Host listening on {transport.ConnectionData.Address}:{transport.ConnectionData.Port}");
        }
    }

    public void StartClient(string ipAddress = "127.0.0.1")
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;
        transport.ConnectionData.Port = 7777;
        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"Client start attempt to {ipAddress}:{transport.ConnectionData.Port}: {(started ? "Success" : "Failed")}");
    }
}