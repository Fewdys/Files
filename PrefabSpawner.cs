using TMPro;  // Import TextMeshPro namespace
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefab;           // The prefab to instantiate
    public Transform targetObject;      // The object to which prefabs will be parented
    public int numberToSpawn = 0;       // How many prefabs to spawn initially
    public TMP_Text textMeshPro;        // Reference to the TextMeshPro component to update

    public List<GameObject> spawnedPrefabs = new List<GameObject>();  // List to track spawned prefabs

    void Start()
    {
        UpdateSpawnedPrefabs();
        UpdateTextMeshPro();
    }

    void Update()
    {
        // Continuously check if the number of prefabs to spawn has changed
        if (spawnedPrefabs.Count != numberToSpawn)
        {
            UpdateSpawnedPrefabs();
            UpdateTextMeshPro();
        }
    }

    // Updates the prefabs in real-time based on the numberToSpawn
    void UpdateSpawnedPrefabs()
    {
        // If we need to reduce the number of prefabs, destroy extra ones
        if (spawnedPrefabs.Count > numberToSpawn)
        {
            int excess = spawnedPrefabs.Count - numberToSpawn;
            for (int i = 0; i < excess; i++)
            {
                DestroyPrefab();
            }
        }
        // If we need to spawn more prefabs, instantiate new ones
        else if (spawnedPrefabs.Count < numberToSpawn)
        {
            int shortfall = numberToSpawn - spawnedPrefabs.Count;
            for (int i = 0; i < shortfall; i++)
            {
                SpawnPrefab(); // Spawn new prefabs
            }
        }
    }

    // Spawns a single prefab and makes it a child of the targetObject
    public GameObject SpawnPrefab()
    {
        // Instantiate the prefab at the target object's position and set it as a child
        GameObject newPrefab = Instantiate(prefab, targetObject.position, Quaternion.identity);
        newPrefab.transform.SetParent(targetObject, false); // Set the targetObject as the parent, keep local scale
        spawnedPrefabs.Add(newPrefab);
        return newPrefab; // Return the newly created prefab for further use
    }

    // Destroys the last prefab in the list
    void DestroyPrefab()
    {
        if (spawnedPrefabs.Count > 0)
        {
            // Get the last prefab in the list
            GameObject prefabToDestroy = spawnedPrefabs[spawnedPrefabs.Count - 1];
            // Remove it from the list and destroy it
            spawnedPrefabs.Remove(prefabToDestroy);
            Destroy(prefabToDestroy);
        }
    }

    // Clears all spawned prefabs
    public void ClearPrefabs()
    {
        foreach (var prefab in spawnedPrefabs)
        {
            Destroy(prefab); // Destroy the prefab
        }
        spawnedPrefabs.Clear(); // Clear the list
    }

    // Sets the username for a specific prefab instance
    public void SetPrefabUsername(GameObject prefabInstance, string username)
    {
        var playerNameText = prefabInstance.transform.Find("PlayerName").GetComponent<TMP_Text>();
        if (playerNameText != null)
        {
            playerNameText.text = username; // Set the username
        }
    }

    // Updates the TextMeshPro with the current number of spawned prefabs
    void UpdateTextMeshPro()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = "Player Count: " + (spawnedPrefabs.Count + 1).ToString();
        }
    }
}
