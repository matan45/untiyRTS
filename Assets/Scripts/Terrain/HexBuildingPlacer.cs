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
    /// </summary>
    public class HexBuildingPlacer : MonoBehaviour
    {
        public static HexBuildingPlacer Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        [Header("Placement Settings")]
        [SerializeField] private float ghostHeightOffset = 0.5f;
        [SerializeField] private LayerMask terrainLayer;

        // Current placement state
        private BuildingData currentBuildingData;
        private GameObject ghostObject;
        private Renderer ghostRenderer;
        private bool isPlacing = false;
        private Vector2Int currentHexCoord;
        private bool isValidPlacement = false;

        // Events
        public event System.Action<GameObject, Vector2Int> OnBuildingPlaced;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (mainCamera == null)
                mainCamera = Camera.main;

            CreateDefaultMaterials();
        }

        private void Update()
        {
            if (isPlacing && ghostObject != null)
            {
                UpdateGhostPosition();
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
                currentHexCoord = hexCoord;

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

                // Check if placement is valid
                CheckPlacementValidity();
            }
        }

        private void CheckPlacementValidity()
        {
            isValidPlacement = true;

            // Check if HexGridManager exists
            HexGridManager gridManager = HexGridManager.Instance;
            if (gridManager == null || gridManager.Grid == null)
            {
                isValidPlacement = false;
                UpdateGhostMaterial();
                return;
            }

            // Get tile at current position
            HexTile tile = gridManager.Grid.GetTile(currentHexCoord);
            if (tile == null)
            {
                isValidPlacement = false;
                UpdateGhostMaterial();
                return;
            }

            // Check if tile is buildable
            if (!tile.IsBuildable)
            {
                isValidPlacement = false;
                UpdateGhostMaterial();
                return;
            }

            // Check if hex already occupied
            if (tile.OccupyingBuilding != null)
            {
                isValidPlacement = false;
                UpdateGhostMaterial();
                return;
            }

            // Check resources (requires ResourceManager from existing system)
            if (!CanAffordBuilding())
            {
                isValidPlacement = false;
                UpdateGhostMaterial();
                return;
            }

            // All checks passed
            isValidPlacement = true;
            UpdateGhostMaterial();
        }

        private bool CanAffordBuilding()
        {
            // Try to find ResourceManager from existing system
            var resourceManager = FindFirstObjectByType<ResourceManager>();
            if (resourceManager != null && currentBuildingData != null)
            {
                return resourceManager.CanAfford(
                    currentBuildingData.creditsCost,
                    currentBuildingData.powerRequired
                );
            }

            // If no ResourceManager, assume affordable (for testing)
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
            var resourceManager = FindFirstObjectByType<ResourceManager>();
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
            }

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
            }
        }

        public bool IsCurrentlyPlacing => isPlacing;
    }
}
