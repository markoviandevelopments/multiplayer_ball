using Unity.Netcode;
using UnityEngine;
using TMPro;

public class SphereController : NetworkBehaviour
{
    public bool isHostSphere;
    public float moveSpeed = 5f;
    public float maxSpeed = 10f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.6f;
    public float rotationSpeed = 10000f;
    public float groundedDrag = 0.5f;
    public float airborneDrag = 0f;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isLooking;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Camera playerCamera; // Assign HostCamera for Player1, ClientCamera for Player2

    private NetworkVariable<bool> isMessageVisible = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isFirstPerson = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (messageText == null)
        {
            messageText = transform.Find("MessageCanvas/MessageText")?.GetComponent<TextMeshProUGUI>();
            if (messageText == null)
            {
                Debug.LogError("MessageText not found on " + gameObject.name);
            }
        }

        // Assign camera by name
        if (playerCamera == null)
        {
            string cameraName = isHostSphere ? "HostCamera" : "ClientCamera";
            GameObject cameraObj = GameObject.Find(cameraName);
            if (cameraObj != null)
            {
                playerCamera = cameraObj.GetComponent<Camera>();
                Debug.Log($"Assigned {playerCamera.name} to {gameObject.name} (isHostSphere: {isHostSphere}, IsServer: {IsServer})");
            }
            else
            {
                Debug.LogError($"No camera found with name {cameraName} for {gameObject.name} (isHostSphere: {isHostSphere}, IsServer: {IsServer})");
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsSpawned) return;

        bool canControl = false;
        if (isHostSphere)
        {
            canControl = IsOwner;
        }
        else
        {
            canControl = IsOwner && !IsServer;
        }

        if (canControl)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
            rb.drag = isGrounded ? groundedDrag : airborneDrag;
            Debug.Log($"Drag set to {rb.drag} for {gameObject.name} (Grounded: {isGrounded})");

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (isFirstPerson.Value)
            {
                if (playerCamera != null)
                {
                    
                    float willheimer;
                    if (isLooking)
                    {
                        willheimer = 0.0f;
                    }
                    else
                    {
                        willheimer = 1.0f;
                    }
                    float rotation = horizontal * rotationSpeed * Time.fixedDeltaTime * willheimer;
                    float rotation2 = horizontal * rotationSpeed * Time.fixedDeltaTime * (1.0f - willheimer);
                    playerCamera.transform.Rotate(rotation2, rotation, 0f);
                    Vector3 moveDirection = playerCamera.transform.forward * vertical;
                    rb.AddForce(moveDirection * moveSpeed, ForceMode.Force);
                }
            }
            else
            {
                Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
                rb.AddForce(moveDirection * moveSpeed);
            }

            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                Debug.Log($"Jumped {gameObject.name} with force {jumpForce}");
            }

            if (rb.position.y < -55f)
            {
                rb.position = new Vector3(0f, 0.5f, 0f);
                rb.linearVelocity = Vector3.zero;
                Debug.Log($"Teleported {gameObject.name} to (0, 0.5, 0)");
            }

            isMessageVisible.Value = Input.GetKey(KeyCode.T);

            if (Input.GetKeyDown(KeyCode.L))
            {
                groundedDrag = 0.5f - groundedDrag;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                moveSpeed *= 2;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                moveSpeed /= 2;
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                isLooking = false;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                isLooking = true;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                isFirstPerson.Value = !isFirstPerson.Value;
                Debug.Log($"Toggled first-person mode for {gameObject.name}: {isFirstPerson.Value}");
            }

            Debug.Log($"Speed: {rb.velocity.magnitude}, Position: {rb.position}, Rotation: {transform.rotation.eulerAngles}");
        }

        if (messageText != null)
        {
            messageText.enabled = isMessageVisible.Value;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }

    public bool IsFirstPerson()
    {
        return isFirstPerson.Value;
    }
}