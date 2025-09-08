using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionUIManager : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField ipInputField;
    public NetworkHandler networkHandler;

    void Start()
    {
        // Automatically find NetworkHandler if not assigned
        if (networkHandler == null)
        {
            networkHandler = FindObjectOfType<NetworkHandler>();
            if (networkHandler == null)
            {
                Debug.LogError("NetworkHandler not found in scene! Please assign in Inspector or ensure NetworkHandlerObject exists.");
                return;
            }
            Debug.Log("NetworkHandler found automatically.");
        }

        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
        ipInputField.text = "192.168.1.126";
    }

    void OnHostButtonClicked()
    {
        networkHandler.StartHost();
        gameObject.SetActive(false);
        Debug.Log("Starting as Host...");
    }

    void OnClientButtonClicked()
    {
        string ip = ipInputField.text;
        if (string.IsNullOrEmpty(ip))
        {
            ip = "192.168.1.126";
            Debug.LogWarning("No IP entered, using localhost.");
        }
        networkHandler.StartClient(ip);
        gameObject.SetActive(false);
        Debug.Log($"Attempting to join as Client at IP: {ip}");
    }
}