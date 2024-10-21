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
    private TMP_Text currentlySelectedButtonText; // Text reference of the currently selected button
    private string currentlySelectedUsername; // Reference to the currently selected player's username

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

    // Event handler for button click
    void OnButtonClicked(Button button, TMP_Text buttonText, string username)
    {
        // Check if the clicked button is already selected
        if (currentlySelectedButtonText == buttonText)
        {
            // If it is already selected, deselect it
            buttonText.text = "Select";
            currentlySelectedButtonText = null;  // Clear the selection
            currentlySelectedUsername = null; // Clear the currently selected player
            GUIFunctions.selectedplayer = null; // Clear the selected player reference
        }
        else
        {
            // If another button is selected, set that to "Select"
            if (currentlySelectedButtonText != null)
            {
                currentlySelectedButtonText.text = "Select";
            }

            // Set the clicked button's text to "Deselect" and update the currently selected reference
            buttonText.text = "Deselect";
            currentlySelectedButtonText = buttonText;  // Update the reference to the currently selected button text
            currentlySelectedUsername = username; // Update the currently selected player's username
            foreach (var player in CVRPlayerManager.Instance.NetworkPlayers)
            {
                if (player.Username == username)
                GUIFunctions.selectedplayer = player; // Set the selected player
            }
        }
    }

    public void SetPrefabUsername(GameObject prefabInstance, string username)
    {
        var playerNameText = prefabInstance.transform.Find("PlayerName").GetComponent<TMP_Text>();
        if (playerNameText != null)
        {
            playerNameText.richText = true;
            playerNameText.text = username; // Set the username
        }
    }

    // Spawns a single prefab and makes it a child of the targetObject
    public GameObject SpawnPrefab()
    {
        // Instantiate the prefab at the target object's position and set it as a child
        GameObject newPrefab = Instantiate(prefab, targetObject.position, Quaternion.identity);
        newPrefab.transform.SetParent(targetObject, false); // Set the targetObject as the parent, keep local scale
        spawnedPrefabs.Add(newPrefab);
        newPrefab.gameObject.SetActive(true);

        // Set up the button in the prefab
        Button button = newPrefab.transform.Find("Button").GetComponent<Button>();
        TMP_Text buttonText = button.transform.Find("ButtonText").GetComponent<TMP_Text>();

        // Assume the username is set in the prefab's PlayerName field
        string username = newPrefab.transform.Find("PlayerName").GetComponent<TMP_Text>().text;

        // Subscribe to the button's click event
        button.onClick.AddListener(() => OnButtonClicked(button, buttonText, username));

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

    // Updates the TextMeshPro with the current number of spawned prefabs
    void UpdateTextMeshPro()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = "Player Count: " + (spawnedPrefabs.Count + 1).ToString();
        }
    }
}
