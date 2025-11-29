using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Terrain.Core;
using RTS.Terrain.Data;

namespace RTS.Terrain.Rendering
{
    /// <summary>
    /// Visual feedback component for territory ownership display.
    /// Creates border overlays at territory boundaries.
    /// Uses object pooling and event-driven updates for performance.
    /// </summary>
    public class TerritoryOwnershipVisual : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("Territory visual configuration")]
        private TerritoryVisualConfigSO visualConfig;

        [SerializeField, Tooltip("Player colors configuration")]
        private PlayerColorsConfigSO playerColors;

        [Header("References")]
        [SerializeField, Tooltip("Reference to the HexGridManager")]
        private HexGridManager gridManager;

        // Object pooling
        private Queue<GameObject> _borderPool = new Queue<GameObject>();

        // Active borders: tile coordinates -> border GameObject
        private Dictionary<Vector2Int, GameObject> _activeBorders = new Dictionary<Vector2Int, GameObject>();

        // Per-player materials (cached for performance)
        private Dictionary<int, Material> _playerMaterials = new Dictionary<int, Material>();

        // Tiles that need border updates (for batch processing)
        private HashSet<Vector2Int> _pendingUpdates = new HashSet<Vector2Int>();

        private bool _isInitialized;

        private void Start()
        {
            StartCoroutine(InitializeAfterGridReady());
        }

        private IEnumerator InitializeAfterGridReady()
        {
            // Use serialized reference if available, otherwise find instance
            HexGridManager manager = gridManager != null ? gridManager : HexGridManager.Instance;

            // Wait for grid manager to be available and have tiles
            int waitFrames = 0;
            while (manager == null || manager.Grid == null || !HasTiles(manager.Grid))
            {
                waitFrames++;
                if (waitFrames > 300) // Timeout after 5 seconds at 60fps
                {
                    Debug.LogError("TerritoryOwnershipVisual: Timeout waiting for grid.");
                    yield break;
                }
                // Re-check each frame in case it becomes available
                if (manager == null)
                {
                    manager = gridManager != null ? gridManager : HexGridManager.Instance;
                }
                yield return null;
            }

            // Assign to the field for later use
            gridManager = manager;

            // Wait one more frame to ensure everything is settled
            yield return null;

            Initialize();
        }

        private bool HasTiles(HexGrid grid)
        {
            foreach (var _ in grid.GetAllTiles())
                return true;
            return false;
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            // Find grid manager if not assigned
            if (gridManager == null)
            {
                gridManager = HexGridManager.Instance;
            }

            if (gridManager == null || gridManager.Grid == null)
            {
                Debug.LogWarning("TerritoryOwnershipVisual: HexGridManager not found. Territory borders disabled.");
                return;
            }

            InitializePool();
            SubscribeToAllTiles();
            RefreshAllBorders();

            _isInitialized = true;
        }

        private int GetTileCount()
        {
            int count = 0;
            if (gridManager?.Grid != null)
            {
                foreach (var _ in gridManager.Grid.GetAllTiles())
                    count++;
            }
            return count;
        }

        private void OnEnable()
        {
            if (_isInitialized && gridManager != null)
            {
                SubscribeToAllTiles();
                RefreshAllBorders();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromAllTiles();
            HideAllBorders();
        }

        /// <summary>
        /// Initialize the object pool for border GameObjects.
        /// </summary>
        private void InitializePool()
        {
            int poolSize = visualConfig != null ? visualConfig.initialPoolSize : 50;

            for (int i = 0; i < poolSize; i++)
            {
                var border = CreateBorderObject();
                border.SetActive(false);
                _borderPool.Enqueue(border);
            }
        }

        /// <summary>
        /// Create a single border GameObject.
        /// </summary>
        private GameObject CreateBorderObject()
        {
            var go = new GameObject("TerritoryBorder");
            go.transform.SetParent(transform);

            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            return go;
        }

        /// <summary>
        /// Get a border from the pool or create a new one.
        /// </summary>
        private GameObject GetBorderFromPool()
        {
            if (_borderPool.Count > 0)
            {
                return _borderPool.Dequeue();
            }
            return CreateBorderObject();
        }

        /// <summary>
        /// Return a border to the pool.
        /// </summary>
        private void ReturnBorderToPool(GameObject border)
        {
            if (border != null)
            {
                border.SetActive(false);
                _borderPool.Enqueue(border);
            }
        }

        /// <summary>
        /// Get or create a material for a specific player.
        /// </summary>
        private Material GetPlayerMaterial(int playerId)
        {
            if (_playerMaterials.TryGetValue(playerId, out Material cachedMaterial))
            {
                return cachedMaterial;
            }

            // Get base color from player colors config
            Color baseColor = playerColors != null
                ? playerColors.GetPlayerColor(playerId)
                : GetDefaultPlayerColor(playerId);

            // Apply alpha from visual config
            Color finalColor = visualConfig != null
                ? visualConfig.ApplyAlpha(baseColor)
                : new Color(baseColor.r, baseColor.g, baseColor.b, 0.85f);

            // Create material
            Material material = CreateBorderMaterial(finalColor);
            _playerMaterials[playerId] = material;

            return material;
        }

        /// <summary>
        /// Create a border material with the specified color.
        /// Uses URP/Unlit for simple, visible solid color borders.
        /// </summary>
        private Material CreateBorderMaterial(Color color)
        {
            // Always use Unlit shader for borders - simple, visible, performant
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);

            // Set color
            material.color = color;
            material.SetColor("_BaseColor", color);

            // URP transparency setup for Unlit shader
            material.SetFloat("_Surface", 1); // 1 = Transparent
            material.SetFloat("_Blend", 0); // 0 = Alpha
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_Cull", 0); // Cull Off - render both sides

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 10;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            return material;
        }

        /// <summary>
        /// Get default player color when no config is available.
        /// </summary>
        private Color GetDefaultPlayerColor(int playerId)
        {
            return playerId switch
            {
                0 => new Color(0.2f, 0.4f, 0.8f), // Blue
                1 => new Color(0.8f, 0.2f, 0.2f), // Red
                2 => new Color(0.2f, 0.7f, 0.3f), // Green
                3 => new Color(0.9f, 0.75f, 0.1f), // Yellow
                _ => Color.gray
            };
        }

        /// <summary>
        /// Subscribe to ownership change events for all tiles.
        /// </summary>
        private void SubscribeToAllTiles()
        {
            if (gridManager?.Grid == null) return;

            foreach (var tile in gridManager.Grid.GetAllTiles())
            {
                tile.OnOwnershipChanged += (prevOwner, newOwner) => OnTileOwnershipChanged(tile, prevOwner, newOwner);
            }
        }

        /// <summary>
        /// Unsubscribe from all tile events.
        /// </summary>
        private void UnsubscribeFromAllTiles()
        {
            // Note: In a production environment, we'd need to store delegates to properly unsubscribe.
            // For now, we rely on tiles being cleared/destroyed with the grid.
        }

        /// <summary>
        /// Handle tile ownership change event.
        /// </summary>
        private void OnTileOwnershipChanged(HexTile tile, int previousOwner, int newOwner)
        {

            // Queue this tile and its neighbors for border update
            QueueBorderUpdate(tile.Coordinates);

            // Also update neighbors (their edge masks may have changed)
            for (int i = 0; i < 6; i++)
            {
                Vector2Int neighborCoord = HexCoordinates.GetNeighborCoordinate(tile.Coordinates, i);
                QueueBorderUpdate(neighborCoord);
            }
        }

        /// <summary>
        /// Queue a tile for border update (processed in LateUpdate).
        /// </summary>
        private void QueueBorderUpdate(Vector2Int coordinates)
        {
            _pendingUpdates.Add(coordinates);
        }

        private void LateUpdate()
        {
            // Process pending border updates
            if (_pendingUpdates.Count > 0)
            {
                foreach (var coord in _pendingUpdates)
                {
                    UpdateBorderForTile(coord);
                }
                _pendingUpdates.Clear();
            }
        }

        /// <summary>
        /// Update the border for a specific tile.
        /// </summary>
        private void UpdateBorderForTile(Vector2Int coordinates)
        {
            if (gridManager?.Grid == null) return;

            HexTile tile = gridManager.Grid.GetTile(coordinates);

            // Remove existing border if any
            if (_activeBorders.TryGetValue(coordinates, out GameObject existingBorder))
            {
                ReturnBorderToPool(existingBorder);
                _activeBorders.Remove(coordinates);
            }

            // Check if tile needs a border
            if (tile == null || !tile.HasOwner)
            {
                return;
            }

            // Calculate edge mask
            int edgeMask = TerritoryBorderMeshGenerator.CalculateEdgeMask(tile, gridManager.Grid);

            if (edgeMask == 0)
            {
                // No edges to render (tile is fully surrounded by same-owner tiles)
                return;
            }

            // Create border
            ShowBorder(tile, edgeMask);
        }

        /// <summary>
        /// Show a border for a tile with the specified edge mask.
        /// </summary>
        private void ShowBorder(HexTile tile, int edgeMask)
        {
            GameObject border = GetBorderFromPool();

            // Get mesh for this edge configuration
            float outerRadius = HexCoordinates.HexSize;
            float borderWidth = visualConfig != null ? visualConfig.borderWidth : 0.12f;
            int segments = visualConfig != null ? visualConfig.hexVertexCount : 6;

            Mesh mesh = TerritoryBorderMeshGenerator.GetOrCreatePartialBorder(outerRadius, borderWidth, edgeMask, segments);

            var meshFilter = border.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            // Set material
            var meshRenderer = border.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = GetPlayerMaterial(tile.OwnerId);

            // Position the border
            PositionBorder(border, tile);

            border.SetActive(true);
            _activeBorders[tile.Coordinates] = border;
        }

        /// <summary>
        /// Position a border GameObject on a tile.
        /// </summary>
        private void PositionBorder(GameObject border, HexTile tile)
        {
            float heightOffset = visualConfig != null ? visualConfig.heightOffset : 0.03f;

            // Get tile surface height
            var tileObj = gridManager?.GetTileObject(tile.Coordinates);
            float y = tileObj != null ? tileObj.GetTopSurfaceY() : tile.WorldPosition.y;

            border.transform.position = new Vector3(
                tile.WorldPosition.x,
                y + heightOffset,
                tile.WorldPosition.z
            );
        }

        /// <summary>
        /// Refresh borders for all tiles in the grid.
        /// </summary>
        public void RefreshAllBorders()
        {
            if (gridManager?.Grid == null) return;

            // Clear existing borders
            HideAllBorders();

            // Update all owned tiles
            foreach (var tile in gridManager.Grid.GetAllTiles())
            {
                if (tile.HasOwner)
                {
                    int edgeMask = TerritoryBorderMeshGenerator.CalculateEdgeMask(tile, gridManager.Grid);
                    if (edgeMask > 0)
                    {
                        ShowBorder(tile, edgeMask);
                    }
                }
            }
        }

        /// <summary>
        /// Hide all active borders.
        /// </summary>
        private void HideAllBorders()
        {
            foreach (var border in _activeBorders.Values)
            {
                ReturnBorderToPool(border);
            }
            _activeBorders.Clear();
        }

        private void OnDestroy()
        {
            // Clean up materials
            foreach (var material in _playerMaterials.Values)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            _playerMaterials.Clear();

            // Clean up mesh cache
            TerritoryBorderMeshGenerator.ClearCache();
        }
    }
}
