using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public ObjectSpawner objectSpawner; 
    public FormChangingSpawner formChangingSpawner; 
    public PlayerChanger playerChanger;
    public List<GameObject> spawnedObjects = new List<GameObject>();

    private bool hasStartedSpawning = false;
    public bool isGameActive = true; 
    public float spawnInterval; 
    public float fallSpeed;
    private int spawnThreshold;

    void Start()
    {
        if (isGameActive)
        {
            StartSpawning();
            spawnThreshold = Random.Range(2, 5);

        }
    }

    void Update()
    {
        if (isGameActive && !hasStartedSpawning)
        {
            StartSpawning();
        }
        else if (!isGameActive && hasStartedSpawning)
        {
            StopSpawning();
        }
    }

    public void SpawnObject()
    {

        if (objectSpawner != null && formChangingSpawner != null)
        {
            if (playerChanger.score >= spawnThreshold)
            {
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    objectSpawner.SpawnObject();
                }
                else
                {
                    formChangingSpawner.SpawnObject();
                }
            }
            else
            {
                objectSpawner.SpawnObject();
            }
        }
        else
        {
            Debug.LogWarning("ObjectSpawner veya FormChangingSpawner referansý bulunamadý!");
        }
    }

    public void StartSpawning(float delay = 1f)
    {
        if (!hasStartedSpawning)
        {
            CancelInvoke("SpawnObject"); 
            InvokeRepeating("SpawnObject", delay, spawnInterval);
            hasStartedSpawning = true;
        }
    }

    public void StopSpawning()
    {
        CancelInvoke("SpawnObject");
        hasStartedSpawning = false;
    }

    public void ClearSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    public void ResetSpawner()
    {
        StopSpawning();
        ClearSpawnedObjects();

        spawnInterval = 6f;
        fallSpeed = 2f;
    }

    public void IncreaseFallSpeed(float amount)
    {
        fallSpeed += amount;
        foreach (var obj in spawnedObjects)
        {
            if (obj != null && obj.TryGetComponent(out FallingObject fo))
            {
                fo.fallSpeed = fallSpeed;
            }
        }
    }

    public void DecreaseSpawnInterval(float amount, float minSpawnInterval = 0.5f)
    {
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - amount);
        CancelInvoke("SpawnObject");
        InvokeRepeating("SpawnObject", 1f, spawnInterval);
    }
}
