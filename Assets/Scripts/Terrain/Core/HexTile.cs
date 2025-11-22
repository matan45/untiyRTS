using UnityEngine;
using RTS.Terrain;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Represents a single hexagonal tile in the grid.
    /// Tracks occupancy, terrain type, and world position.
    /// </summary>
    public class HexTile
    {
        public Vector2Int Coordinates { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public TerrainType TerrainType { get; set; }
        public bool IsBuildable { get; set; }
        public GameObject OccupyingBuilding { get; private set; }

        public HexTile(Vector2Int coordinates, Vector3 worldPosition)
        {
            Coordinates = coordinates;
            WorldPosition = worldPosition;
            TerrainType = TerrainType.Grassland;
            IsBuildable = true;
            OccupyingBuilding = null;
        }

        public HexTile(int q, int r, Vector3 worldPosition)
            : this(new Vector2Int(q, r), worldPosition)
        {
        }

        /// <summary>
        /// Mark this tile as occupied by a building.
        /// </summary>
        public void SetOccupyingBuilding(GameObject building)
        {
            OccupyingBuilding = building;
        }

        /// <summary>
        /// Clear the occupying building.
        /// </summary>
        public void ClearOccupyingBuilding()
        {
            OccupyingBuilding = null;
        }

        /// <summary>
        /// Check if this tile is occupied.
        /// </summary>
        public bool IsOccupied => OccupyingBuilding != null;

        /// <summary>
        /// Get the world position of this tile.
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            return WorldPosition;
        }
    }
}
