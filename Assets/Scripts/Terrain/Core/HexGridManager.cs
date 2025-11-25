using UnityEngine;
using RTS.Terrain;

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

        public HexGrid Grid { get; private set; }

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

        private void InitializeGrid()
        {
            Grid = new HexGrid(gridWidth, gridHeight);

            // Create tiles for existing 3D terrain
            // This will be populated by Hex3DTerrainGenerator or manually
            Debug.Log($"HexGridManager: Initialized {gridWidth}x{gridHeight} grid");
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
