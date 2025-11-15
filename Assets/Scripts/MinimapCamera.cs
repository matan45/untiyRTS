using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Header("Minimap Settings")]
    [SerializeField] private float height = 100f;
    [SerializeField] private float mapSize = 100f; // Should match your map boundaries
    
    [Header("References")]
    [SerializeField] private Transform targetToFollow; // Main camera transform
    
    void Start()
    {
        // Position the minimap camera above the center of the map
        transform.position = new Vector3(0, height, 0);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Set orthographic size based on map size
        Camera cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = mapSize / 2f;
        }
    }
    
    void LateUpdate()
    {
        // Optionally follow the main camera's X and Z position
        if (targetToFollow != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = targetToFollow.position.x;
            newPosition.z = targetToFollow.position.z;
            transform.position = newPosition;
        }
    }
}