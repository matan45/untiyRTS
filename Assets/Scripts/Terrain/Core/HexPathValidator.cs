using UnityEngine;
using RTS.Terrain.Data;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Default path validator for ground units.
    /// Uses TerrainMovementConfigSO for data-driven movement costs.
    /// Falls back to hardcoded defaults if no config is provided.
    /// </summary>
    public class HexPathValidator : IPathValidator
    {
        private readonly HexGrid _grid;
        private readonly TerrainMovementConfigSO _movementConfig;

        /// <summary>
        /// Create a path validator with optional movement configuration.
        /// </summary>
        /// <param name="grid">The hex grid to validate against</param>
        /// <param name="movementConfig">Optional movement configuration (null uses defaults)</param>
        public HexPathValidator(HexGrid grid, TerrainMovementConfigSO movementConfig = null)
        {
            _grid = grid;
            _movementConfig = movementConfig;
        }

        /// <summary>
        /// Check if a tile exists and is passable.
        /// </summary>
        public bool IsPassable(Vector2Int coordinates)
        {
            HexTile tile = _grid.GetTile(coordinates);
            if (tile == null) return false;

            return _movementConfig != null
                ? _movementConfig.IsPassable(tile.TerrainType)
                : DefaultIsPassable(tile.TerrainType);
        }

        /// <summary>
        /// Get movement cost from tile properties.
        /// Returns float.MaxValue for impassable tiles.
        /// </summary>
        public float GetMovementCost(Vector2Int coordinates)
        {
            HexTile tile = _grid.GetTile(coordinates);
            if (tile == null) return float.MaxValue;

            return _movementConfig != null
                ? _movementConfig.GetMovementCost(tile.TerrainType)
                : DefaultMovementCost(tile.TerrainType);
        }

        /// <summary>
        /// Validate a move between adjacent tiles.
        /// Checks: tiles exist, adjacent, destination passable, destination not occupied.
        /// </summary>
        public bool IsValidMove(Vector2Int from, Vector2Int to)
        {
            // Check both tiles exist
            HexTile fromTile = _grid.GetTile(from);
            HexTile toTile = _grid.GetTile(to);

            if (fromTile == null || toTile == null) return false;

            // Check destination is passable
            if (!IsPassable(to)) return false;

            // Check destination not occupied by a building
            if (toTile.IsOccupied) return false;

            // Verify tiles are adjacent (hex distance of 1)
            if (HexCoordinates.Distance(from, to) != 1) return false;

            return true;
        }

        /// <summary>
        /// Fallback passability check when no config is provided.
        /// Water, deep water, and mountains are impassable for ground units.
        /// </summary>
        private bool DefaultIsPassable(TerrainType type)
        {
            return type != TerrainType.Water &&
                   type != TerrainType.DeepWater &&
                   type != TerrainType.Mountains;
        }

        /// <summary>
        /// Fallback movement cost when no config is provided.
        /// Returns sensible defaults for each terrain type.
        /// </summary>
        private float DefaultMovementCost(TerrainType type)
        {
            return type switch
            {
                TerrainType.Grassland => 1f,
                TerrainType.Plains => 1f,
                TerrainType.Desert => 1.5f,
                TerrainType.Tundra => 1.5f,
                TerrainType.Snow => 2f,
                TerrainType.Hills => 2f,
                TerrainType.Forest => 1.5f,
                TerrainType.Swamp => 2.5f,
                TerrainType.Mountains => float.MaxValue,
                TerrainType.Water => float.MaxValue,
                TerrainType.DeepWater => float.MaxValue,
                _ => 1f
            };
        }
    }
}
