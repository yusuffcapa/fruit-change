using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SpecialObject : MonoBehaviour
{
    public List<GameObject> spawnedObjects = new List<GameObject>();
    public PlayerChanger playerChanger; 
    public GameObject objectToSpawnPrefab1; 
    public GameObject objectToSpawnPrefab2; 
    public float spawnHeight; 
    public float fallSpeed;
    public float spawnInterval;
    private bool hasStartedSpawning = false;
    private int spawnThreshold;
    public AudioSource audioSource;
    public AudioClip coconutSound;

    private void Start()
    {
        if (playerChanger != null)
        {
            playerChanger = playerChanger.GetComponent<PlayerChanger>();
            spawnThreshold = Random.Range(5, 15);
        }
    }

    void Update()
    {
        if (!hasStartedSpawning && playerChanger != null && playerChanger.score > spawnThreshold)
        {
            StartSpawning();
        }

        if (playerChanger.isTouchingObject)
        {
            return;
        }
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    touchPosition.z = 0f;

                    List<GameObject> objectsToChange = new List<GameObject>();

                    foreach (var spawnedObject in spawnedObjects)
                    {
                        if (spawnedObject != null && spawnedObject.GetComponent<Collider2D>() != null && spawnedObject.GetComponent<Collider2D>().OverlapPoint(touchPosition))
                        {
                            objectsToChange.Add(spawnedObject);
                        }
                    }

                    foreach (var obj in objectsToChange)
                    {
                        ChangePrefab(obj);
                    }
                }
            }
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

    void SpawnObject()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-1.5f, 1.5f), spawnHeight, transform.position.z);
        GameObject spawnedObject = Instantiate(objectToSpawnPrefab1, spawnPosition, Quaternion.identity);

        FallingObject falling = spawnedObject.AddComponent<FallingObject>();

        falling.SetFallSpeed(fallSpeed);
        spawnedObjects.Add(spawnedObject);
    }

    void ChangePrefab(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        spawnedObjects.Remove(obj);

        GameObject newObj = Instantiate(objectToSpawnPrefab2, position, Quaternion.identity);
        spawnedObjects.Add(newObj);
        audioSource.PlayOneShot(coconutSound);

        Destroy(obj);

        if (playerChanger.isFakeFormActive) return;

        if (playerChanger != null)
        {
            playerChanger.score += 3;
            playerChanger.UpdateScoreUI(); 
        }

            Destroy(newObj, 0.5f);
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
            {
                Destroy(obj);
            }
        }

        spawnedObjects.Clear();
    }
}
