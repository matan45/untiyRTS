using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Data;
using RTS.Buildings;
using RTS.UI;

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

    [Header("Rotation Settings")]
    [Tooltip("Sensitivity of rotation when dragging middle mouse button")]
    [SerializeField] private float rotationSensitivity = 0.5f;

    [Header("UI References")]
    [Tooltip("UI Text to display rotation angle")]
    [SerializeField] private TMPro.TextMeshProUGUI rotationAngleText;

    private BuildingData currentBuilding;
    private GameObject ghostObject;
    private bool isPlacing = false;
    private bool isValidPlacement = false;

    private Camera mainCamera;
    private InputAction mousePositionAction;
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction cancelAction;
    private InputAction middleMouseAction;

    private Material[] originalMaterials;

    // Rotation tracking
    private float currentRotation = 0f;
    private bool isRotating = false;
    private Vector2 lastMousePosition;

    // Performance optimization: Cache validity state to avoid updating materials every frame
    private bool lastValidityState = false;

    // Performance optimization: Cache renderers to avoid GetComponentsInChildren every frame
    private Renderer[] cachedRenderers;

    // Material rendering mode constants (Unity Standard Shader)
    private const float MATERIAL_MODE_TRANSPARENT = 3f;
    private const int MATERIAL_ZWRITE_DISABLED = 0;
    private const int MATERIAL_RENDER_QUEUE_TRANSPARENT = 3000;

    // URP Shader constants
    private const float URP_SURFACE_TRANSPARENT = 1f; // 0 = Opaque, 1 = Transparent
    private const float URP_BLEND_ALPHA = 0f;         // 0 = Alpha blending
    
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
                rightClickAction = cameraMap.FindAction("RightClick");
                cancelAction = inputActions.FindActionMap("Camera").FindAction("Rotate");
                middleMouseAction = cameraMap.FindAction("MiddleMouseDrag");

                if (leftClickAction != null)
                {
                    leftClickAction.performed += OnLeftClick;
                }

                if (rightClickAction != null)
                {
                    rightClickAction.performed += OnRightClick;
                }

                if (middleMouseAction != null)
                {
                    middleMouseAction.started += OnMiddleMousePressed;
                    middleMouseAction.canceled += OnMiddleMouseReleased;
                }
            }
        }
    }
    
    void OnDisable()
    {
        if (leftClickAction != null)
            leftClickAction.performed -= OnLeftClick;

        if (rightClickAction != null)
            rightClickAction.performed -= OnRightClick;

        if (middleMouseAction != null)
        {
            middleMouseAction.started -= OnMiddleMousePressed;
            middleMouseAction.canceled -= OnMiddleMouseReleased;
        }
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
            UpdateGhostRotation();
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

        // Reset rotation
        currentRotation = 0f;
        isRotating = false;

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

        // Cache renderers for performance (avoid GetComponentsInChildren every frame)
        cachedRenderers = ghostObject.GetComponentsInChildren<Renderer>();

        foreach (var renderer in cachedRenderers)
        {
            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = validPlacementMaterial != null ? validPlacementMaterial : renderer.materials[i];
            }
            renderer.materials = mats;
        }

        // Initialize validity state
        lastValidityState = false;
    }
    
    private void UpdateGhostPosition()
    {
        // Don't update position while rotating
        if (isRotating)
            return;

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

    private void UpdateGhostRotation()
    {
        if (!isRotating || ghostObject == null || mousePositionAction == null)
        {
            return;
        }

        // Calculate mouse delta manually
        Vector2 currentMousePos = mousePositionAction.ReadValue<Vector2>();
        Vector2 mouseDelta = currentMousePos - lastMousePosition;

        // Rotate based on horizontal mouse movement
        currentRotation += mouseDelta.x * rotationSensitivity;

        // Normalize rotation to 0-360 range
        currentRotation = currentRotation % 360f;
        if (currentRotation < 0f)
            currentRotation += 360f;

        // Apply rotation to ghost object
        ghostObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

        // Update last mouse position
        lastMousePosition = currentMousePos;

        // Update UI
        UpdateRotationUI();
    }

    private void OnMiddleMousePressed(InputAction.CallbackContext context)
    {
        if (isPlacing && mousePositionAction != null)
        {
            isRotating = true;
            // Initialize last mouse position for delta calculation
            lastMousePosition = mousePositionAction.ReadValue<Vector2>();
        }
    }

    private void OnMiddleMouseReleased(InputAction.CallbackContext context)
    {
        isRotating = false;
    }

    private void UpdateRotationUI()
    {
        if (rotationAngleText != null && isPlacing)
        {
            // Use string.Format to avoid string concatenation allocations in Update
            rotationAngleText.SetText("Rotation: {0}\u00b0", Mathf.RoundToInt(currentRotation));
            rotationAngleText.gameObject.SetActive(true);
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

        // Performance optimization: Only update materials when validity state changes
        if (isValidPlacement != lastValidityState)
        {
            UpdateGhostMaterial(isValidPlacement);
            lastValidityState = isValidPlacement;
        }
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
            // Ignore ghost object hierarchy first
            if (col.transform.IsChildOf(ghostObject.transform) || col.transform == ghostObject.transform)
                continue;

            // CRITICAL FIX: Check for buildings BEFORE filtering by ground layer
            // This prevents buildings from being filtered out if they're on the ground layer
            // Use GetComponentInParent to search up the hierarchy for Building component
            Building otherBuilding = col.GetComponentInParent<Building>();
            if (otherBuilding != null)
            {
                return true; // Collision with another building - invalid placement
            }

            // Now filter out ground layer (after checking for buildings)
            if (IsGroundLayer(col.gameObject.layer))
                continue;
            
            return true;
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
        // Use cached renderers to avoid GetComponentsInChildren call
        if (cachedRenderers == null || cachedRenderers.Length == 0)
            return;

        Color targetColor = valid ? validPlacementColor : invalidPlacementColor;
        Material targetMaterial = valid ? validPlacementMaterial : invalidPlacementMaterial;

        foreach (var renderer in cachedRenderers)
        {
            if (renderer == null)
                continue;

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
    /// Supports both Standard shader and URP (Universal Render Pipeline) shaders.
    /// </summary>
    private void SetMaterialTransparent(Material material)
    {
        if (material == null) return;

        // Detect shader type and apply appropriate transparency settings
        bool isURPShader = material.HasProperty("_Surface") ||
                          material.shader.name.Contains("Universal Render Pipeline");

        if (isURPShader)
        {
            // URP Lit shader transparency settings
            material.SetFloat("_Surface", URP_SURFACE_TRANSPARENT);
            material.SetFloat("_Blend", URP_BLEND_ALPHA);

            // Set blend modes for URP
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", MATERIAL_ZWRITE_DISABLED);

            // Enable URP transparency keywords
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            // Standard shader transparency settings
            material.SetFloat("_Mode", MATERIAL_MODE_TRANSPARENT);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", MATERIAL_ZWRITE_DISABLED);

            // Enable Standard shader transparency keywords
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        // Set render queue to transparent (common for both shader types)
        material.renderQueue = MATERIAL_RENDER_QUEUE_TRANSPARENT;
    }
    
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (isPlacing && isValidPlacement)
        {
            PlaceBuilding();
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (isPlacing)
        {
            CancelPlacement();
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

        // IMPORTANT: Ensure colliders are enabled on placed building
        // This prevents buildings from being placed on top of each other
        Collider[] buildingColliders = building.GetComponentsInChildren<Collider>();
        foreach (var col in buildingColliders)
        {
            col.enabled = true;
        }

        // Add to build queue
        bool addedToQueue = false;
        if (BuildQueue.Instance != null)
        {
            addedToQueue = BuildQueue.Instance.AddToQueue(building.GetComponent<Building>(), currentBuilding);
        }
        else
        {
            // No queue manager, consider it successful
            addedToQueue = true;
        }

        // If queue is full, refund resources and destroy the building
        if (!addedToQueue)
        {
            // Refund the resources
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.RefundResources(currentBuilding.creditsCost, currentBuilding.powerRequired);
            }

            // Destroy the building we just created
            Destroy(building);

            // Show error message to user in UI
            if (UIMessageDisplay.Instance != null)
            {
                UIMessageDisplay.Instance.ShowWarning("Build queue is full! Maximum 5 buildings allowed.");
            }
            else
            {
                // Fallback to console if UI not available
                Debug.LogWarning("Build queue is full! Maximum 5 buildings allowed.");
            }

            // Don't cancel placement - let the user try again or cancel manually
            return;
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
        isRotating = false;
        currentRotation = 0f;

        // Clear cached data
        cachedRenderers = null;
        lastValidityState = false;

        // Hide rotation UI
        if (rotationAngleText != null)
        {
            rotationAngleText.gameObject.SetActive(false);
        }

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