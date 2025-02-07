using Game.ServerAuthoritativeSynchronousSpawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneModifier : MonoBehaviour
{
    public Button spawnButton;
    public Button hideButton;
    public GameObject objectToSpawn;
    private bool canSpawn = false;
    private bool canHide = false;
    [SerializeField] private GameObject objectTable;
    [SerializeField] private XRServerAuthoritativeSynchronousSpawning xRServerAuthoritativeSynchronousSpawning;

    void Start()
    {
        // Assign button click listeners
        spawnButton.onClick.AddListener(ToggleSpawnRaycast);
        hideButton.onClick.AddListener(ToggleHideRaycast);
    }

    void Update()
    {
        if (canSpawn)
        {
            // Spawn the object at the specified location
            xRServerAuthoritativeSynchronousSpawning.SpawnObjectfromClientServerRpc(objectTable.transform.position + Vector3.up *0.2f, Quaternion.identity);
            ToggleSpawnRaycast(); // Disable spawn after one successful spawn
        }
        if ((canHide) && Input.GetMouseButtonDown(0))
        {
            PerformRaycast();
        }
    }

    void ToggleSpawnRaycast()
    {
        canSpawn = !canSpawn;
        spawnButton.GetComponent<Image>().color = canSpawn ? Color.green : Color.white;
        canHide = false; // Disable hide functionality if spawn is enabled
    }

    void ToggleHideRaycast()
    {
        canHide = !canHide;
        hideButton.GetComponent<Image>().color = canHide ? Color.green : Color.white;
        canSpawn = false; // Disable spawn functionality if hide is enabled
    }

    void PerformRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the collided object has the AR tag
            if (hit.collider.CompareTag("ARSpawnable"))
            { 
                // Hide the AR-tagged object
                //need to hide on all clients 
                xRServerAuthoritativeSynchronousSpawning.HideObjectfromClientServerRpc(hit.collider.gameObject.name);
                ToggleHideRaycast(); // Disable hide after one successful hide
            }
        }
    }
}
