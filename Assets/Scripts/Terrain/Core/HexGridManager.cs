using System.Collections.Generic;
using UnityEngine;
using RTS.Terrain;
using RTS.Terrain.Data;
using RTS.Terrain.Rendering;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Singleton manager for the hex grid system.
    /// Provides global access to the grid and hex utilities.
    /// NOTE: This is a SCENE-SPECIFIC singleton - each level has its own grid.
    /// The instance will be destroyed when loading a new scene.
    /// </summary>
    public class HexGridManager : MonoBehaviour
    {
        public static HexGridManager Instance { get; private set; }

        [Header("Singleton Settings")]
        [SerializeField, Tooltip("If true, this manager will persist across scene loads. For RTS levels, this should typically be FALSE.")]
        private bool persistAcrossScenes = false;

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 50;
        [SerializeField] private int gridHeight = 50;
        [SerializeField] private float hexSize = 1f;

        [Header("Terrain Configuration")]
        [SerializeField, Tooltip("Registry containing all terrain type configurations")]
        private TerrainTypeRegistry terrainRegistry;

        [Header("Terrain Generation")]
        [SerializeField, Tooltip("Noise scale for terrain generation")]
        private float noiseScale = 0.1f;

        [SerializeField, Tooltip("Seed for terrain generation (0 = random seed each time)")]
        private int seed = 0;

        [SerializeField, Tooltip("Generate terrain on Start")]
        private bool generateOnStart = true;

        private Vector2 _noiseOffset;

        [Header("Rendering")]
        [SerializeField, Tooltip("Grid renderer for creating tile visuals")]
        private HexGridRenderer gridRenderer;

        [SerializeField, Tooltip("Auto-render grid after generation")]
        private bool autoRenderGrid = true;

        public HexGrid Grid { get; private set; }

        /// <summary>
        /// Get the grid renderer for visual operations.
        /// </summary>
        public HexGridRenderer GridRenderer => gridRenderer;

        /// <summary>
        /// Get the terrain type registry for configuration lookups.
        /// </summary>
        public TerrainTypeRegistry TerrainRegistry => terrainRegistry;

        private void Awake()
        {
            // Singleton pattern with optional persistence
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"HexGridManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Optional persistence (typically false for scene-specific grids)
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("HexGridManager: Set to persist across scenes.");
            }

            // Set hex size for coordinate conversions
            HexCoordinates.HexSize = hexSize;

            // Initialize grid
            InitializeGrid();
        }

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateTerrain();
            }
        }

        private void InitializeGrid()
        {
            Grid = new HexGrid(gridWidth, gridHeight);
            Debug.Log($"HexGridManager: Initialized {gridWidth}x{gridHeight} grid");
        }

        /// <summary>
        /// Generate terrain tiles using Perlin noise.
        /// </summary>
        [ContextMenu("Generate Terrain")]
        public void GenerateTerrain()
        {
            if (Grid == null)
            {
                InitializeGrid();
            }

            // Generate noise offset from seed
            int activeSeed = seed != 0 ? seed : System.Environment.TickCount;
            var rng = new System.Random(activeSeed);
            _noiseOffset = new Vector2((float)rng.NextDouble() * 10000f, (float)rng.NextDouble() * 10000f);

            // Generate hex tiles with terrain types based on noise
            for (int q = 0; q < gridWidth; q++)
            {
                for (int r = 0; r < gridHeight; r++)
                {
                    Vector3 worldPos = HexCoordinates.AxialToWorld(q, r);

                    // Get terrain type based on noise with offset for variety
                    float noise = Mathf.PerlinNoise(q * noiseScale + _noiseOffset.x, r * noiseScale + _noiseOffset.y);
                    TerrainType terrainType = GetTerrainFromNoise(noise);

                    // Create tile
                    HexTile tile = new HexTile(q, r, worldPos);
                    tile.TerrainType = terrainType;

                    // Set buildability based on terrain type
                    tile.IsBuildable = terrainType != TerrainType.Water &&
                                       terrainType != TerrainType.DeepWater &&
                                       terrainType != TerrainType.Mountains;

                    // Apply terrain config if available
                    if (terrainRegistry != null)
                    {
                        var config = terrainRegistry.GetConfig(terrainType);
                        if (config != null)
                        {
                            tile.SetTerrainConfig(config);
                        }
                    }

                    Grid.AddTile(tile);
                }
            }

            Debug.Log($"HexGridManager: Generated {gridWidth * gridHeight} terrain tiles");

            // Auto-render if enabled
            if (autoRenderGrid)
            {
                RenderGrid();
            }
        }

        /// <summary>
        /// Get terrain type from noise value.
        /// Uses multiple noise samples for varied terrain distribution.
        /// </summary>
        private TerrainType GetTerrainFromNoise(float noise)
        {
            // Water types (low elevation)
            if (noise < 0.15f)
                return TerrainType.DeepWater;
            else if (noise < 0.25f)
                return TerrainType.Water;
            else if (noise < 0.30f)
                return TerrainType.Swamp;
            // Low elevation land
            else if (noise < 0.45f)
                return TerrainType.Grassland;
            else if (noise < 0.55f)
                return TerrainType.Plains;
            else if (noise < 0.62f)
                return TerrainType.Forest;
            // Mid elevation
            else if (noise < 0.70f)
                return TerrainType.Desert;
            else if (noise < 0.78f)
                return TerrainType.Hills;
            // High elevation
            else if (noise < 0.85f)
                return TerrainType.Tundra;
            else if (noise < 0.92f)
                return TerrainType.Snow;
            else
                return TerrainType.Mountains;
        }

        /// <summary>
        /// Clear and regenerate terrain.
        /// </summary>
        [ContextMenu("Regenerate Terrain")]
        public void RegenerateTerrain()
        {
            if (gridRenderer != null)
            {
                gridRenderer.ClearRenderedTiles();
            }

            Grid = new HexGrid(gridWidth, gridHeight);
            GenerateTerrain();
        }

        /// <summary>
        /// Render the grid visuals. Call after grid is populated with tiles.
        /// </summary>
        public void RenderGrid()
        {
            if (gridRenderer == null)
            {
                Debug.LogWarning("HexGridManager: No grid renderer assigned, cannot render grid");
                return;
            }

            if (Grid == null)
            {
                Debug.LogError("HexGridManager: Grid is null, cannot render");
                return;
            }

            gridRenderer.RenderGrid(Grid);
            Debug.Log("HexGridManager: Triggered grid rendering");
        }

        /// <summary>
        /// Update the visual for a specific tile after terrain change.
        /// </summary>
        public void UpdateTileVisual(Vector2Int coordinates)
        {
            if (gridRenderer != null)
            {
                gridRenderer.UpdateTileVisual(coordinates);
            }
        }

        /// <summary>
        /// Get the tile object at specific coordinates.
        /// </summary>
        public HexTileObject GetTileObject(Vector2Int coordinates)
        {
            return gridRenderer?.GetTileObject(coordinates);
        }

        /// <summary>
        /// Register a 3D terrain tile with the grid.
        /// Called by terrain generators.
        /// </summary>
        public void RegisterTerrainTile(int q, int r, GameObject terrainObject, TerrainType terrainType)
        {
            Vector3 worldPos = HexCoordinates.AxialToWorld(q, r);

            // Use the actual terrain object's Y position if available
            if (terrainObject != null)
            {
                worldPos.y = terrainObject.transform.position.y;
            }

            HexTile tile = new HexTile(q, r, worldPos);
            tile.TerrainType = terrainType;

            // Set buildability based on terrain type
            tile.IsBuildable = terrainType != TerrainType.Water &&
                               terrainType != TerrainType.DeepWater &&
                               terrainType != TerrainType.Mountains;

            Grid.AddTile(tile);
        }

        /// <summary>
        /// Get a hex tile at world position.
        /// </summary>
        public HexTile GetTileAtWorldPosition(Vector3 worldPos)
        {
            Vector2Int axialCoords = HexCoordinates.WorldToAxial(worldPos);
            return Grid?.GetTile(axialCoords);
        }

        #region Save/Load Methods

        /// <summary>
        /// Save the current grid state to a serializable data object.
        /// </summary>
        public HexGridSaveData SaveGrid()
        {
            if (Grid == null)
            {
                Debug.LogError("HexGridManager: Cannot save - Grid is null");
                return null;
            }

            var tiles = new List<HexTileData>();
            foreach (var tile in Grid.GetAllTiles())
            {
                tiles.Add(tile.ToData());
            }

            var saveData = new HexGridSaveData(gridWidth, gridHeight)
            {
                tiles = tiles.ToArray()
            };

            Debug.Log($"HexGridManager: Saved {tiles.Count} tiles");
            return saveData;
        }

        /// <summary>
        /// Load grid state from serialized data.
        /// </summary>
        public void LoadGrid(HexGridSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogError("HexGridManager: Cannot load - saveData is null");
                return;
            }

            if (!saveData.Validate())
            {
                Debug.LogError("HexGridManager: Cannot load - saveData validation failed");
                return;
            }

            int loadedCount = 0;
            foreach (var data in saveData.tiles)
            {
                var tile = Grid.GetTile(data.q, data.r);
                if (tile != null)
                {
                    var config = terrainRegistry?.GetConfig(data.terrainType);
                    tile.ApplyData(data, config);
                    loadedCount++;
                }
                else
                {
                    Debug.LogWarning($"HexGridManager: No tile found at ({data.q}, {data.r}) during load");
                }
            }

            Debug.Log($"HexGridManager: Loaded {loadedCount} tiles from save data");
        }

        /// <summary>
        /// Export the grid to a JSON string.
        /// </summary>
        public string ExportToJson(bool prettyPrint = false)
        {
            var saveData = SaveGrid();
            if (saveData == null)
                return null;

            return saveData.ToJson(prettyPrint);
        }

        /// <summary>
        /// Import the grid from a JSON string.
        /// </summary>
        public bool ImportFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("HexGridManager: Cannot import - JSON string is empty");
                return false;
            }

            var saveData = HexGridSaveData.FromJson(json);
            if (saveData == null)
            {
                Debug.LogError("HexGridManager: Cannot import - failed to parse JSON");
                return false;
            }

            LoadGrid(saveData);
            return true;
        }

        /// <summary>
        /// Get a terrain configuration for a specific terrain type.
        /// </summary>
        public TerrainTypeDataSO GetTerrainConfig(TerrainType terrainType)
        {
            return terrainRegistry?.GetConfig(terrainType);
        }

        #endregion

        private void OnDestroy()
        {
            // Clear static instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
