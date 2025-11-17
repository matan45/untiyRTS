using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Data;
using RTS.Buildings;

public class BuildingPlacer : MonoBehaviour
{
    public static BuildingPlacer Instance { get; private set; }

    // Event fired when a building is placed (per CLAUDE.md: event-driven architecture)
    public event System.Action OnBuildingPlaced;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool useGridSnapping = true;

    [Header("Ghost Visual Settings")]
    [Tooltip("Color tint for valid placement")]
    [SerializeField] private Color validPlacementColor = new Color(1f, 1f, 1f, 0.5f); // White, semi-transparent

    [Tooltip("Color tint for invalid placement")]
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f); // Red, semi-transparent

    [Header("Collision Settings")]
    [Tooltip("Maximum slope angle (degrees) allowed for building placement")]
    [SerializeField] private float maxSlopeAngle = 30f;

    [Tooltip("Height above ground to check for collisions")]
    [SerializeField] private float collisionCheckHeight = 5f;
    
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    
    private BuildingData currentBuilding;
    private GameObject ghostObject;
    private bool isPlacing = false;
    private bool isValidPlacement = false;

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

        // Improved collision and slope checks
        bool hasCollision = CheckForBuildingCollisions();
        bool validSlope = CheckTerrainSlope();

        isValidPlacement = hasResources && !hasCollision && validSlope;

        // Update material color
        UpdateGhostMaterial(isValidPlacement);
    }
    
    /// <summary>
    /// Checks for collisions with other buildings and obstacles.
    /// Uses improved box cast with proper vertical sizing.
    /// </summary>
    private bool CheckForBuildingCollisions()
    {
        if (ghostObject == null || currentBuilding == null)
            return true;

        Vector3 center = ghostObject.transform.position + Vector3.up * (collisionCheckHeight / 2f);
        Vector3 halfExtents = new Vector3(
            currentBuilding.size.x / 2f,
            collisionCheckHeight / 2f,
            currentBuilding.size.y / 2f
        );

        // Get all colliders in the building footprint
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, ghostObject.transform.rotation);

        // Check for buildings and obstacles (not ground)
        foreach (var col in colliders)
        {
            // Ignore ground and ghost objects
            if (col.gameObject.name == "Ground" || col.gameObject.name.StartsWith("Ghost_"))
                continue;

            // Check if it's another building - STRICT CHECK to prevent building on buildings
            Building otherBuilding = col.GetComponent<Building>();
            if (otherBuilding != null)
            {
                return true; // Collision with another building - invalid placement
            }

            // Also block placement on any other non-ground collider (rocks, trees, etc.)
            if (!IsGroundLayer(col.gameObject.layer))
            {
                return true;
            }
        }

        return false; // No collisions - valid placement
    }

    /// <summary>
    /// Checks if the terrain slope at the building position is acceptable.
    /// Samples multiple points across the building footprint.
    /// </summary>
    private bool CheckTerrainSlope()
    {
        if (ghostObject == null || currentBuilding == null)
            return false;

        Vector3 center = ghostObject.transform.position;
        float sizeX = currentBuilding.size.x;
        float sizeZ = currentBuilding.size.y;

        // Sample points across the building footprint
        Vector3[] samplePoints = new Vector3[]
        {
            center, // Center
            center + new Vector3(sizeX/2, 0, 0), // Right
            center + new Vector3(-sizeX/2, 0, 0), // Left
            center + new Vector3(0, 0, sizeZ/2), // Forward
            center + new Vector3(0, 0, -sizeZ/2), // Back
            center + new Vector3(sizeX/2, 0, sizeZ/2), // Front-right corner
            center + new Vector3(-sizeX/2, 0, sizeZ/2), // Front-left corner
            center + new Vector3(sizeX/2, 0, -sizeZ/2), // Back-right corner
            center + new Vector3(-sizeX/2, 0, -sizeZ/2) // Back-left corner
        };

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        int validHits = 0;

        // Raycast down from each sample point to find terrain height
        foreach (Vector3 point in samplePoints)
        {
            Vector3 rayStart = point + Vector3.up * 10f; // Start above
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                minHeight = Mathf.Min(minHeight, hit.point.y);
                maxHeight = Mathf.Max(maxHeight, hit.point.y);
                validHits++;

                // Check slope at this point using terrain normal
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > maxSlopeAngle)
                {
                    return false; // Slope too steep at this point
                }
            }
        }

        // Need at least half the sample points to hit ground
        if (validHits < samplePoints.Length / 2)
            return false;

        // Check height variance (too much = uneven ground)
        float heightVariance = maxHeight - minHeight;
        float maxAllowedVariance = currentBuilding.size.x * 0.2f; // 20% of building size

        return heightVariance <= maxAllowedVariance;
    }

    /// <summary>
    /// Checks if a layer is the ground layer.
    /// </summary>
    private bool IsGroundLayer(int layer)
    {
        return ((1 << layer) & groundLayer) != 0;
    }
    
    private void UpdateGhostMaterial(bool valid)
    {
        if (ghostObject == null)
            return;

        Color targetColor = valid ? validPlacementColor : invalidPlacementColor;
        Material targetMaterial = valid ? validPlacementMaterial : invalidPlacementMaterial;

        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // If custom materials are assigned, use them
            if (targetMaterial != null)
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = targetMaterial;
                }
                renderer.materials = mats;
            }

            // Always apply color tint (works with or without custom materials)
            foreach (var mat in renderer.materials)
            {
                if (mat != null)
                {
                    // Set color property - works for Standard shader and most others
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = targetColor;
                    }

                    // Enable transparency if needed
                    if (targetColor.a < 1f)
                    {
                        SetMaterialTransparent(mat);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Configures a material to support transparency.
    /// </summary>
    private void SetMaterialTransparent(Material material)
    {
        if (material == null) return;

        // Set rendering mode to Transparent (for Standard shader)
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
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

        // Fire event to notify subscribers (per CLAUDE.md: event-driven architecture)
        OnBuildingPlaced?.Invoke();

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

        if (BuildingMenuController.Instance != null)
        {
            BuildingMenuController.Instance.DeselectBuilding();
        }
    }

    public bool IsPlacing()
    {
        return isPlacing;
    }
}