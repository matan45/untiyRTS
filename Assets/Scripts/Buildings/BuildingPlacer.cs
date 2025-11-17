using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Data;
using RTS.Buildings;

public class BuildingPlacer : MonoBehaviour
{
    public static BuildingPlacer Instance { get; private set; }
    
    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool useGridSnapping = true;
    
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    
    private BuildingData currentBuilding;
    private GameObject ghostObject;
    private bool isPlacing = false;
    private bool isValidPlacement = false;
    private float lastPlacementTime = -1f;

    private Camera mainCamera;
    private InputAction mousePositionAction;
    private InputAction leftClickAction;
    private InputAction cancelAction;

    private Material[] originalMaterials;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        SetupInput();
    }
    
    void OnEnable()
    {
        if (inputActions != null)
        {
            var cameraMap = inputActions.FindActionMap("Camera");
            if (cameraMap != null)
            {
                mousePositionAction = cameraMap.FindAction("MousePosition");
                leftClickAction = cameraMap.FindAction("LeftClick");
                cancelAction = inputActions.FindActionMap("Camera").FindAction("Rotate");

                if (leftClickAction != null)
                {
                    leftClickAction.performed += OnLeftClick;
                }
            }
        }
    }
    
    void OnDisable()
    {
        if (leftClickAction != null)
            leftClickAction.performed -= OnLeftClick;
    }
    
    private void SetupInput()
    {
        // Input will be setup in OnEnable
    }
    
    void Update()
    {
        if (isPlacing && ghostObject != null)
        {
            UpdateGhostPosition();
            CheckPlacementValidity();
            
            // Cancel placement with ESC
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
            }
        }
    }
    
    public void StartPlacement(BuildingData building)
    {
        if (building == null || building.prefab == null)
            return;

        currentBuilding = building;
        isPlacing = true;

        // Create ghost object
        ghostObject = Instantiate(building.prefab);
        ghostObject.name = "Ghost_" + building.buildingName;

        // Make it semi-transparent
        SetupGhostMaterials();
        
        // Disable colliders on ghost
        Collider[] colliders = ghostObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable any scripts
        MonoBehaviour[] scripts = ghostObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            script.enabled = false;
        }
    }
    
    private void SetupGhostMaterials()
    {
        if (ghostObject == null)
            return;
            
        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = validPlacementMaterial != null ? validPlacementMaterial : renderer.materials[i];
            }
            renderer.materials = mats;
        }
    }
    
    private void UpdateGhostPosition()
    {
        if (mousePositionAction == null || mainCamera == null)
            return;
            
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 position = hit.point;
            
            // Apply grid snapping
            if (useGridSnapping)
            {
                position.x = Mathf.Round(position.x / gridSize) * gridSize;
                position.z = Mathf.Round(position.z / gridSize) * gridSize;
            }
            
            ghostObject.transform.position = position;
        }
    }
    
    private void CheckPlacementValidity()
    {
        // Check if position is valid (no collisions, enough resources, etc.)
        bool hasResources = ResourceManager.Instance != null &&
            ResourceManager.Instance.CanAfford(currentBuilding.creditsCost, currentBuilding.powerRequired);

        // Simple collision check - can be improved
        bool hasCollision = CheckForCollisions();

        isValidPlacement = hasResources && !hasCollision;

        // Update material color
        UpdateGhostMaterial(isValidPlacement);
    }
    
    private bool CheckForCollisions()
    {
        if (ghostObject == null)
            return true;

        // Simple overlap sphere check
        Collider[] colliders = Physics.OverlapBox(
            ghostObject.transform.position,
            new Vector3(currentBuilding.size.x / 2f, 0.5f, currentBuilding.size.y / 2f),
            ghostObject.transform.rotation
        );

        // Filter out the ground plane - we only care about collisions with other buildings/obstacles
        int validCollisionCount = 0;
        foreach (var col in colliders)
        {
            // Ignore ground plane and ghost objects
            if (col.gameObject.name == "Ground" || col.gameObject.name.StartsWith("Ghost_"))
            {
                continue;
            }

            validCollisionCount++;
        }

        return validCollisionCount > 0;
    }
    
    private void UpdateGhostMaterial(bool valid)
    {
        if (ghostObject == null)
            return;
            
        Material targetMaterial = valid ? validPlacementMaterial : invalidPlacementMaterial;
        
        if (targetMaterial == null)
            return;
            
        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = targetMaterial;
            }
            renderer.materials = mats;
        }
    }
    
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (isPlacing && isValidPlacement)
        {
            PlaceBuilding();
        }
    }
    
    public void PlaceBuilding()
    {
        if (!isValidPlacement || ghostObject == null || currentBuilding == null)
            return;
            
        // Spend resources
        if (ResourceManager.Instance != null)
        {
            if (!ResourceManager.Instance.SpendResources(currentBuilding.creditsCost, currentBuilding.powerRequired))
                return;
        }
        
        // Create actual building
        GameObject building = Instantiate(currentBuilding.prefab, ghostObject.transform.position, ghostObject.transform.rotation);
        
        // Add to build queue
        if (BuildQueue.Instance != null)
        {
            BuildQueue.Instance.AddToQueue(building.GetComponent<Building>(), currentBuilding);
        }
        
        CancelPlacement();
    }
    
    public void CancelPlacement()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }

        ghostObject = null;
        currentBuilding = null;
        isPlacing = false;
        isValidPlacement = false;
        lastPlacementTime = Time.time;

        if (BuildingMenuController.Instance != null)
        {
            BuildingMenuController.Instance.DeselectBuilding();
        }
    }

    public bool IsPlacing()
    {
        return isPlacing;
    }

    public bool JustPlacedBuilding()
    {
        // Return true for a short time after placement to prevent immediate selection
        return Time.time - lastPlacementTime < 0.1f;
    }
}