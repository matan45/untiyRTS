using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Terrain.Core;
using RTS.Data;

namespace RTS.Terrain
{
    /// <summary>
    /// Handles building placement on hex grid.
    /// Snaps buildings to hex centers, validates terrain type, checks occupancy.
    /// Single-hex buildings only, no rotation.
    /// NOTE: This is a SCENE-SPECIFIC singleton - placement logic is per-level.
    /// The instance will be destroyed when loading a new scene.
    /// </summary>
    public class HexBuildingPlacer : MonoBehaviour
    {
        public static HexBuildingPlacer Instance { get; private set; }

        [Header("Singleton Settings")]
        [SerializeField, Tooltip("If true, this placer will persist across scene loads. Typically FALSE for scene-specific gameplay.")]
        private bool persistAcrossScenes = false;

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField, Tooltip("Material for valid placement preview. If not assigned, will be created at runtime.")]
        private Material validPlacementMaterial;
        [SerializeField, Tooltip("Material for invalid placement preview. If not assigned, will be created at runtime.")]
        private Material invalidPlacementMaterial;

        [Header("Placement Settings")]
        [SerializeField] private float ghostHeightOffset = 0.5f;
        [SerializeField] private LayerMask terrainLayer;

        [Header("Performance Settings")]
        [SerializeField, Tooltip("Update ghost position every N frames (1 = every frame, 2 = every other frame, etc.). Higher values improve performance.")]
        [Range(1, 5)] private int updateInterval = 1;
        [SerializeField, Tooltip("Use Input System actions for better performance (recommended).")]
        private bool useInputSystemActions = true;

        // Current placement state
        private BuildingData currentBuildingData;
        private GameObject ghostObject;
        private Renderer ghostRenderer;
        private bool isPlacing = false;
        private Vector2Int currentHexCoord;
        private Vector2Int lastHexCoord = new Vector2Int(int.MinValue, int.MinValue);
        private bool isValidPlacement = false;
        private int frameCounter = 0;

        // Cached references
        private ResourceManager resourceManager;

        // Runtime material tracking (for cleanup)
        private bool validPlacementMaterialCreatedAtRuntime = false;
        private bool invalidPlacementMaterialCreatedAtRuntime = false;

        // Input System actions (for event-driven input)
        private InputAction placeAction;
        private InputAction cancelAction;

        // Events
        public event System.Action<GameObject, Vector2Int> OnBuildingPlaced;

        private void Awake()
        {
            // Singleton pattern with optional persistence
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"HexBuildingPlacer: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Optional persistence (typically false for scene-specific placement)
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("HexBuildingPlacer: Set to persist across scenes.");
            }

            if (mainCamera == null)
                mainCamera = Camera.main;

            // Cache manager references
            resourceManager = FindFirstObjectByType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogWarning("HexBuildingPlacer: ResourceManager not found in scene. Building costs will not be validated.");
            }

            // Set up Input System actions for event-driven input
            if (useInputSystemActions)
            {
                SetupInputActions();
            }

            CreateDefaultMaterials();
        }

        private void Update()
        {
            if (!isPlacing || ghostObject == null)
                return;

            // Throttle updates for performance (only update every N frames)
            frameCounter++;
            if (frameCounter >= updateInterval)
            {
                frameCounter = 0;
                UpdateGhostPosition();
            }

            // Handle input polling (only if not using Input System actions)
            if (!useInputSystemActions)
            {
                HandlePlacementInput();
            }
        }

        /// <summary>
        /// Start placement mode with the selected building.
        /// </summary>
        public void StartPlacement(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                Debug.LogError("HexBuildingPlacer: Building data is null!");
                return;
            }

            currentBuildingData = buildingData;
            CreateGhostObject();
            isPlacing = true;

            // Enable input actions if using Input System
            if (useInputSystemActions)
            {
                placeAction?.Enable();
                cancelAction?.Enable();
            }

            // Reset frame counter for immediate update
            frameCounter = updateInterval;

            Debug.Log($"Started placing: {buildingData.buildingName}");
        }

        /// <summary>
        /// Cancel current placement.
        /// </summary>
        public void CancelPlacement()
        {
            if (ghostObject != null)
            {
                Destroy(ghostObject);
            }

            ghostObject = null;
            ghostRenderer = null;
            currentBuildingData = null;
            isPlacing = false;
            isValidPlacement = false;
            lastHexCoord = new Vector2Int(int.MinValue, int.MinValue);

            // Disable input actions if using Input System
            if (useInputSystemActions)
            {
                placeAction?.Disable();
                cancelAction?.Disable();
            }

            Debug.Log("Placement cancelled");
        }

        private void CreateGhostObject()
        {
            if (currentBuildingData == null || currentBuildingData.prefab == null)
            {
                Debug.LogError("HexBuildingPlacer: Building prefab is null!");
                return;
            }

            // Instantiate ghost from prefab
            ghostObject = Instantiate(currentBuildingData.prefab);
            ghostObject.name = "Ghost_" + currentBuildingData.buildingName;

            // Disable all scripts on ghost (it's just a visual preview)
            foreach (MonoBehaviour script in ghostObject.GetComponentsInChildren<MonoBehaviour>())
            {
                script.enabled = false;
            }

            // Disable colliders
            foreach (Collider col in ghostObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Get renderer for material swapping
            ghostRenderer = ghostObject.GetComponentInChildren<Renderer>();

            // Make semi-transparent
            if (ghostRenderer != null)
            {
                ghostRenderer.material = validPlacementMaterial;
            }
        }

        private void UpdateGhostPosition()
        {
            // Raycast from mouse to terrain
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, terrainLayer))
            {
                // Convert world position to hex coordinate
                Vector2Int hexCoord = HexCoordinates.WorldToAxial(hit.point);

                // Only update if hex coordinate changed (optimization)
                bool hexChanged = hexCoord != lastHexCoord;
                if (hexChanged)
                {
                    currentHexCoord = hexCoord;
                    lastHexCoord = hexCoord;

                    // Get hex center world position
                    Vector3 hexCenter = HexCoordinates.AxialToWorld(hexCoord);

                    // Check if hex grid manager exists
                    HexGridManager gridManager = HexGridManager.Instance;
                    if (gridManager != null && gridManager.Grid != null)
                    {
                        HexTile tile = gridManager.Grid.GetTile(hexCoord);
                        if (tile != null)
                        {
                            // Use tile's actual height
                            hexCenter.y = tile.GetWorldPosition().y + ghostHeightOffset;
                        }
                    }

                    // Position ghost at hex center
                    ghostObject.transform.position = hexCenter;

                    // Check if placement is valid (only when position changes)
                    CheckPlacementValidity();
                }
            }
        }

        private void CheckPlacementValidity()
        {
            bool wasValid = isValidPlacement;
            isValidPlacement = true;

            // Check if HexGridManager exists
            HexGridManager gridManager = HexGridManager.Instance;
            if (gridManager == null || gridManager.Grid == null)
            {
                isValidPlacement = false;
            }
            else
            {
                // Get tile at current position
                HexTile tile = gridManager.Grid.GetTile(currentHexCoord);
                if (tile == null)
                {
                    isValidPlacement = false;
                }
                else if (!tile.IsBuildable)
                {
                    // Check if tile is buildable
                    isValidPlacement = false;
                }
                else if (tile.OccupyingBuilding != null)
                {
                    // Check if hex already occupied
                    isValidPlacement = false;
                }
                else if (!CanAffordBuilding())
                {
                    // Check resources
                    isValidPlacement = false;
                }
            }

            // Only update material if validity state changed (optimization)
            if (isValidPlacement != wasValid)
            {
                UpdateGhostMaterial();
            }
        }

        private bool CanAffordBuilding()
        {
            if (currentBuildingData == null)
            {
                Debug.LogError("HexBuildingPlacer: Cannot check affordability - building data is null");
                return false;
            }

            // Use cached ResourceManager reference
            if (resourceManager != null)
            {
                return resourceManager.CanAfford(
                    currentBuildingData.creditsCost,
                    currentBuildingData.powerRequired
                );
            }

            // If no ResourceManager, assume affordable (for testing/single-player without resources)
            // This warning was already logged in Awake()
            return true;
        }

        private void UpdateGhostMaterial()
        {
            if (ghostRenderer == null) return;

            ghostRenderer.material = isValidPlacement ? validPlacementMaterial : invalidPlacementMaterial;
        }

        private void HandlePlacementInput()
        {
            // Left click to place
            if (Mouse.current.leftButton.wasPressedThisFrame && isValidPlacement)
            {
                PlaceBuilding();
            }

            // Right click or ESC to cancel
            if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
            }
        }

        private void PlaceBuilding()
        {
            if (!isValidPlacement || currentBuildingData == null)
                return;

            // Get hex center position
            Vector3 buildingPosition = HexCoordinates.AxialToWorld(currentHexCoord);

            HexGridManager gridManager = HexGridManager.Instance;
            if (gridManager != null && gridManager.Grid != null)
            {
                HexTile tile = gridManager.Grid.GetTile(currentHexCoord);
                if (tile != null)
                {
                    buildingPosition.y = tile.GetWorldPosition().y;
                }
            }

            // Spend resources
            if (resourceManager != null)
            {
                if (!resourceManager.SpendResources(
                    currentBuildingData.creditsCost,
                    currentBuildingData.powerRequired))
                {
                    Debug.LogWarning("Cannot afford building!");
                    return;
                }
            }

            // Instantiate actual building
            GameObject building = Instantiate(
                currentBuildingData.prefab,
                buildingPosition,
                Quaternion.identity // No rotation
            );

            building.name = currentBuildingData.buildingName;

            // Update hex tile occupancy
            if (gridManager != null && gridManager.Grid != null)
            {
                HexTile tile = gridManager.Grid.GetTile(currentHexCoord);
                if (tile != null)
                {
                    tile.SetOccupyingBuilding(building);
                }
            }

            // Fire event
            OnBuildingPlaced?.Invoke(building, currentHexCoord);

            Debug.Log($"Placed {currentBuildingData.buildingName} at hex {currentHexCoord}");

            // Clean up placement mode
            CancelPlacement();
        }

        private void CreateDefaultMaterials()
        {
            // Create valid placement material if not assigned
            if (validPlacementMaterial == null)
            {
                validPlacementMaterial = new Material(Shader.Find("Standard"));
                validPlacementMaterial.color = new Color(1f, 1f, 1f, 0.5f);
                validPlacementMaterial.SetFloat("_Mode", 3); // Transparent
                validPlacementMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                validPlacementMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                validPlacementMaterial.SetInt("_ZWrite", 0);
                validPlacementMaterial.DisableKeyword("_ALPHATEST_ON");
                validPlacementMaterial.EnableKeyword("_ALPHABLEND_ON");
                validPlacementMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                validPlacementMaterial.renderQueue = 3000;

                // Mark as runtime-created for cleanup
                validPlacementMaterialCreatedAtRuntime = true;
            }

            // Create invalid placement material if not assigned
            if (invalidPlacementMaterial == null)
            {
                invalidPlacementMaterial = new Material(Shader.Find("Standard"));
                invalidPlacementMaterial.color = new Color(1f, 0f, 0f, 0.5f);
                invalidPlacementMaterial.SetFloat("_Mode", 3); // Transparent
                invalidPlacementMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                invalidPlacementMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                invalidPlacementMaterial.SetInt("_ZWrite", 0);
                invalidPlacementMaterial.DisableKeyword("_ALPHATEST_ON");
                invalidPlacementMaterial.EnableKeyword("_ALPHABLEND_ON");
                invalidPlacementMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                invalidPlacementMaterial.renderQueue = 3000;

                // Mark as runtime-created for cleanup
                invalidPlacementMaterialCreatedAtRuntime = true;
            }
        }

        public bool IsCurrentlyPlacing => isPlacing;

        /// <summary>
        /// Sets up Input System actions for event-driven input handling.
        /// </summary>
        private void SetupInputActions()
        {
            // Create place action (left mouse button)
            placeAction = new InputAction("Place", InputActionType.Button);
            placeAction.AddBinding("<Mouse>/leftButton");
            placeAction.performed += ctx => OnPlaceActionPerformed();

            // Create cancel action (right mouse button or Escape)
            cancelAction = new InputAction("Cancel", InputActionType.Button);
            cancelAction.AddBinding("<Mouse>/rightButton");
            cancelAction.AddBinding("<Keyboard>/escape");
            cancelAction.performed += ctx => OnCancelActionPerformed();

            // Actions start disabled, enabled in StartPlacement()
        }

        /// <summary>
        /// Called when place action is performed (left click).
        /// </summary>
        private void OnPlaceActionPerformed()
        {
            if (isPlacing && isValidPlacement)
            {
                PlaceBuilding();
            }
        }

        /// <summary>
        /// Called when cancel action is performed (right click or Escape).
        /// </summary>
        private void OnCancelActionPerformed()
        {
            if (isPlacing)
            {
                CancelPlacement();
            }
        }

        private void OnDestroy()
        {
            // Cleanup Input System actions
            if (placeAction != null)
            {
                placeAction.Dispose();
                placeAction = null;
            }

            if (cancelAction != null)
            {
                cancelAction.Dispose();
                cancelAction = null;
            }

            // Destroy runtime-created materials to prevent memory leaks
            if (validPlacementMaterialCreatedAtRuntime && validPlacementMaterial != null)
            {
                Destroy(validPlacementMaterial);
                validPlacementMaterial = null;
            }

            if (invalidPlacementMaterialCreatedAtRuntime && invalidPlacementMaterial != null)
            {
                Destroy(invalidPlacementMaterial);
                invalidPlacementMaterial = null;
            }

            // Clear static instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
