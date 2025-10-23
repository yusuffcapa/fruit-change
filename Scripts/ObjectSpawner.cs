using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectsToSpawn;

    public float yStart = 6f;
    public float yEnd = -6f;
    public float minSpawnInterval = 0.5f;
    public SpawnerManager spawnerManager;


    public void SpawnObject()
    {

        int randomIndex = Random.Range(0, objectsToSpawn.Length);
        GameObject prefabToSpawn = objectsToSpawn[randomIndex];

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"❌ objectsToSpawn[{randomIndex}] null, prefab atanmadı.");
            return;
        }

        Vector3 spawnPosition = new Vector3(transform.position.x, yStart, transform.position.z);
        GameObject obj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        var fall = obj.AddComponent<FallingObject>();
        fall.SetFallSpeed(spawnerManager.fallSpeed);
        fall.yEnd = yEnd;

        spawnerManager.spawnedObjects.Add(obj);
    }

}
