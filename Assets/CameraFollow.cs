using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public Vector2 xBounds = new Vector2(-10f, 10f);
    public Vector2 zBounds = new Vector2(-10f, 10f);
    [SerializeField] private Transform target; // Assign Player1 for HostCamera, Player2 for ClientCamera
    private Vector3 initialCameraPos = new Vector3(0f, 5f, -10f);
    private Vector3 initialSpherePos = new Vector3(0f, 0.5f, 0f);
    private Quaternion defaultRotation = Quaternion.Euler(45f, 0f, 0f);
    private Camera mainCamera;
    private SphereController sphereController;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Disable wrong camera locally
        if (gameObject.name == "HostCamera" && !IsServer)
        {
            gameObject.SetActive(false);
            Debug.Log($"Disabled {gameObject.name} on client");
            return;
        }
        if (gameObject.name == "ClientCamera" && IsServer)
        {
            gameObject.SetActive(false);
            Debug.Log($"Disabled {gameObject.name} on host");
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Camera component not found on " + gameObject.name);
            return;
        }

        if (transform.parent != null)
        {
            transform.SetParent(null);
            Debug.Log($"Camera {gameObject.name} unparented");
        }

        initialCameraPos = mainCamera.transform.position;
        Debug.Log($"Camera {gameObject.name} initial position: {initialCameraPos}, default rotation: {defaultRotation.eulerAngles}");

        if (target == null)
        {
            FindPlayerSphere();
        }
        else if (!target.GetComponent<SphereController>())
        {
            Debug.LogError($"Assigned target {target.name} lacks SphereController on {gameObject.name}");
            target = null;
            FindPlayerSphere();
        }
        else
        {
            sphereController = target.GetComponent<SphereController>();
            Debug.Log($"Camera {gameObject.name} assigned to sphere {target.name} (isHostSphere: {sphereController.isHostSphere}, IsServer: {IsServer})");
        }
    }

    void LateUpdate()
    {
        if (!IsSpawned || target == null || sphereController == null) return;

        Vector3 targetPos = target.position;

        if (sphereController.IsFirstPerson())
        {
            Vector3 cameraPos = targetPos + new Vector3(0f, 0.5f, 0f);
            mainCamera.transform.position = cameraPos;
            Vector3 currentRotation = mainCamera.transform.rotation.eulerAngles;
            mainCamera.transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);
            Debug.Log($"Camera {gameObject.name} in first-person mode at {cameraPos}, rotation {mainCamera.transform.rotation.eulerAngles}");
        }
        else
        {
            bool outsideBounds = targetPos.x < xBounds.x || targetPos.x > xBounds.y ||
                                targetPos.z < zBounds.x || targetPos.z > zBounds.y;

            if (outsideBounds)
            {
                Vector3 sphereOffset = targetPos - initialSpherePos;
                Vector3 newCameraPos = initialCameraPos + new Vector3(sphereOffset.x, 0f, sphereOffset.z);
                mainCamera.transform.position = newCameraPos;
                mainCamera.transform.rotation = defaultRotation;
                Debug.Log($"Camera {gameObject.name} following {target.name} at {newCameraPos}, rotation {defaultRotation.eulerAngles}");
            }
            else
            {
                mainCamera.transform.position = initialCameraPos;
                mainCamera.transform.rotation = defaultRotation;
                Debug.Log($"Camera {gameObject.name} stationary at {initialCameraPos}, sphere at {targetPos}");
            }
        }
    }

    private void FindPlayerSphere()
    {
        var spheres = FindObjectsOfType<SphereController>();
        bool isHostCamera = gameObject.name == "HostCamera";
        foreach (var sphere in spheres)
        {
            if (isHostCamera && sphere.isHostSphere && IsLocalPlayer && IsServer ||
                !isHostCamera && !sphere.isHostSphere && IsLocalPlayer && !IsServer)
            {
                target = sphere.transform;
                sphereController = sphere;
                Debug.Log($"Camera {gameObject.name} assigned to sphere {target.name} (isHostSphere: {sphere.isHostSphere}, IsLocalPlayer: {IsLocalPlayer}, IsServer: {IsServer}) via FindPlayerSphere");
                return;
            }
        }
        Debug.LogWarning($"No matching sphere found for camera {gameObject.name} (isHostCamera: {isHostCamera}, IsLocalPlayer: {IsLocalPlayer}, IsServer: {IsServer})");
    }
}