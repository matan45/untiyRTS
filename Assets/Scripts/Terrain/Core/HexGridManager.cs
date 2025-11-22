using UnityEngine;
using RTS.Terrain;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Singleton manager for the hex grid system.
    /// Provides global access to the grid and hex utilities.
    /// </summary>
    public class HexGridManager : MonoBehaviour
    {
        public static HexGridManager Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 50;
        [SerializeField] private int gridHeight = 50;
        [SerializeField] private float hexSize = 1f;

        public HexGrid Grid { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

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
    }
}
