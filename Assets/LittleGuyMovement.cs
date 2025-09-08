using Unity.Netcode;
using UnityEngine;

public class LittleGuyMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float moveInterval = 1f;
    [SerializeField] private float moveRadius = 2f;
    private float moveTimer;
    private Vector3 targetPosition;
    private NetworkVariable<Color> color = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            moveTimer = moveInterval;
            MoveToRandomPosition();
        }
        // Apply color on spawn and when it changes
        color.OnValueChanged += ApplyColor;
        ApplyColor(Color.white, color.Value); // Apply initial or current color
    }

    void Update()
    {
        if (!IsServer) return;

        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            MoveToRandomPosition();
            moveTimer = moveInterval;
        }

        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void MoveToRandomPosition()
    {
        Vector2 randOffset = Random.insideUnitCircle * moveRadius;
        Vector3 randomPoint = transform.position + new Vector3(randOffset.x, 0f, randOffset.y);
        targetPosition = randomPoint;
        Debug.Log($"LittleGuy {gameObject.name} moving to {targetPosition}");
    }

    private void ApplyColor(Color oldColor, Color newColor)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material newMaterial = new Material(renderer.material);
            newMaterial.color = newColor;
            renderer.material = newMaterial;
            Debug.Log($"Applied color {newColor} to {gameObject.name}");
        }
    }

    public void SetColor(Color newColor)
    {
        if (IsServer)
        {
            color.Value = newColor;
        }
    }
}
