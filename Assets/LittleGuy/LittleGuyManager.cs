using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LittleGuyManager : NetworkBehaviour
{
    [SerializeField] private GameObject littleGuyPrefab;
    [SerializeField] private int initialLittleGuyCount = 3;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float minSpawnDistance = 2f;
    [SerializeField] private int maxPopulation = 50;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float spawnChance = 0.01f;
    [SerializeField] private float deathChance = 0.001f;
    private NetworkList<ulong> littleGuyNetworkIds;

    void Awake()
    {
        littleGuyNetworkIds = new NetworkList<ulong>(new List<ulong>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            for (int i = 0; i < initialLittleGuyCount && littleGuyNetworkIds.Count < maxPopulation; i++)
            {
                SpawnLittleGuyServer();
            }
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (Time.time >= spawnInterval)
        {
            if (Random.value < spawnChance && littleGuyNetworkIds.Count < maxPopulation)
            {
                SpawnLittleGuyServer();
            }
        }

        for (int i = littleGuyNetworkIds.Count - 1; i >= 0; i--)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(littleGuyNetworkIds[i], out NetworkObject littleGuyObj) && littleGuyObj != null)
            {
                if (Random.value < deathChance)
                {
                    Vector3 pos = littleGuyObj.transform.position;
                    littleGuyNetworkIds.RemoveAt(i);
                    littleGuyObj.Despawn();
                    Debug.Log($"Little guy despawned at position: {pos}");
                }
            }
        }
    }

    private void SpawnLittleGuyServer()
    {
        if (littleGuyNetworkIds.Count >= maxPopulation)
        {
            Debug.LogWarning($"Cannot spawn new little guy: Population cap of {maxPopulation} reached.");
            return;
        }

        Vector3 randomPoint;
        int attempts = 0;
        const int maxAttempts = 50;

        do
        {
            Vector2 randOffset = Random.insideUnitCircle * spawnRadius;
            randomPoint = new Vector3(randOffset.x, 0.5f, randOffset.y);

            bool tooClose = false;
            foreach (ulong id in littleGuyNetworkIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject littleGuyObj) && littleGuyObj != null)
                {
                    if (Vector3.Distance(randomPoint, littleGuyObj.transform.position) < minSpawnDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
            }

            if (!tooClose)
            {
                GameObject newLittleGuy = Instantiate(littleGuyPrefab, randomPoint, Quaternion.identity);
                NetworkObject netObj = newLittleGuy.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    littleGuyNetworkIds.Add(netObj.NetworkObjectId);
                    newLittleGuy.GetComponent<LittleGuyMovement>().SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f));
                    Debug.Log($"Spawned little guy at: {randomPoint}");
                }
                else
                {
                    Debug.LogError("LittleGuy prefab missing NetworkObject");
                    Destroy(newLittleGuy);
                }
                return;
            }

            attempts++;
        } while (attempts < maxAttempts);

        Debug.LogWarning($"Could not find valid spawn position after {maxAttempts} attempts.");
    }

    public void AddLittleGuy()
    {
        if (IsServer && littleGuyNetworkIds.Count < maxPopulation)
        {
            SpawnLittleGuyServer();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            foreach (ulong id in littleGuyNetworkIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject netObj) && netObj != null)
                {
                    netObj.Despawn();
                }
            }
            littleGuyNetworkIds.Clear();
        }
        base.OnNetworkDespawn();
    }
}
