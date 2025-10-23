using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class FormChangingSpawner : MonoBehaviour
{
    public GameObject[] formsToCycle;
    public float cycleInterval; 
    public float yStart = 6f;
    public float yEnd = -6f;
    public PlayerChanger player;
    public ObjectSpawner mainSpawner; 
    public SpawnerManager spawnerManager;


    public void SpawnObject()
    {
        if (formsToCycle == null || formsToCycle.Length == 0) return;

        GameObject parentObj = new GameObject("FormChangingObject");
        parentObj.transform.position = new Vector3(transform.position.x, yStart, transform.position.z);

        FallingObject fo = parentObj.AddComponent<FallingObject>();
        fo.SetFallSpeed(spawnerManager.fallSpeed);

        spawnerManager.spawnedObjects.Add(parentObj);
        StartCoroutine(CycleAndFall(parentObj));
    }

    IEnumerator CycleAndFall(GameObject parentObj)
    {
        GameObject currentInstance = null;
        bool hasStoppedChanging = false;
        float formChangeInterval = 0.1f;

        while (parentObj != null && parentObj.transform.position.y > yEnd)
        {
            if (!hasStoppedChanging && parentObj.transform.position.y > 3f)
            {
                if (currentInstance != null)
                    Destroy(currentInstance);

                int randomIndex = Random.Range(0, formsToCycle.Length);
                currentInstance = Instantiate(formsToCycle[randomIndex], parentObj.transform.position, Quaternion.identity, parentObj.transform);
            }
            else if (!hasStoppedChanging && parentObj.transform.position.y <= 3f)
            {
                hasStoppedChanging = true;
            }

            yield return new WaitForSeconds(formChangeInterval);
        }

        if (parentObj != null)
            Destroy(parentObj);
    }

}
