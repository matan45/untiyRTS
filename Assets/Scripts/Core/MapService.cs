using System.Collections.Generic;
using UnityEngine;
using RTS.Terrain;
using RTS.Terrain.Core;
using RTS.Terrain.Data;

namespace RTS.Core
{
    /// <summary>
    /// Clean API facade for accessing map/tile information.
    /// Provides a unified interface for armies, cities, and other systems
    /// to query tile data without directly depending on HexGridManager.
    /// </summary>
    public class MapService
    {
        private readonly HexGridManager _gridManager;

        /// <summary>
        /// Create a new MapService instance.
        /// </summary>
        /// <param name="gridManager">The HexGridManager to wrap</param>
        public MapService(HexGridManager gridManager)
        {
            _gridManager = gridManager;
        }

        #region Grid Properties

        /// <summary>
        /// Width of the grid in tiles.
        /// </summary>
        public int Width => _gridManager.Grid?.Width ?? 0;

        /// <summary>
        /// Height of the grid in tiles.
        /// </summary>
        public int Height => _gridManager.Grid?.Height ?? 0;

        #endregion

        #region Tile Queries

        /// <summary>
        /// Get a tile by its axial coordinates.
        /// </summary>
        /// <param name="coordinates">Axial coordinates (q, r)</param>
        /// <returns>The tile at the coordinates, or null if not found</returns>
        public HexTile GetTile(Vector2Int coordinates)
        {
            return _gridManager.Grid?.GetTile(coordinates);
        }

        /// <summary>
        /// Get a tile by its axial coordinates.
        /// </summary>
        public HexTile GetTile(int q, int r)
        {
            return _gridManager.Grid?.GetTile(q, r);
        }

        /// <summary>
        /// Get a tile at a world position.
        /// </summary>
        /// <param name="worldPos">World position</param>
        /// <returns>The tile at the position, or null if not found</returns>
        public HexTile GetTileAtWorldPosition(Vector3 worldPos)
        {
            return _gridManager.GetTileAtWorldPosition(worldPos);
        }

        /// <summary>
        /// Get all tiles in the grid.
        /// </summary>
        public IEnumerable<HexTile> GetAllTiles()
        {
            return _gridManager.Grid?.GetAllTiles() ?? System.Array.Empty<HexTile>();
        }

        /// <summary>
        /// Get neighboring tiles for a coordinate.
        /// </summary>
        public List<HexTile> GetNeighbors(Vector2Int coordinates)
        {
            return _gridManager.Grid?.GetNeighbors(coordinates) ?? new List<HexTile>();
        }

        /// <summary>
        /// Get neighboring tiles (non-allocating version).
        /// </summary>
        public void GetNeighbors(Vector2Int coordinates, List<HexTile> output)
        {
            if (_gridManager.Grid != null)
            {
                _gridManager.Grid.GetNeighbors(coordinates, output);
            }
            else
            {
                output.Clear();
            }
        }

        #endregion

        #region Ownership Queries

        /// <summary>
        /// Get the owner of a tile.
        /// </summary>
        /// <param name="coordinates">Tile coordinates</param>
        /// <returns>Player ID, or -1 if unowned or tile not found</returns>
        public int GetOwner(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.OwnerId ?? -1;
        }

        /// <summary>
        /// Check if a tile is owned by a specific player.
        /// </summary>
        public bool IsOwnedBy(Vector2Int coordinates, int playerId)
        {
            var tile = GetTile(coordinates);
            return tile?.IsOwnedBy(playerId) ?? false;
        }

        /// <summary>
        /// Get all tiles owned by a specific player.
        /// </summary>
        public IEnumerable<HexTile> GetTilesOwnedBy(int playerId)
        {
            return _gridManager.Grid?.GetTilesOwnedBy(playerId) ?? new List<HexTile>();
        }

        /// <summary>
        /// Get all tiles owned by a specific player (non-allocating).
        /// </summary>
        public void GetTilesOwnedBy(int playerId, List<HexTile> output)
        {
            if (_gridManager.Grid != null)
            {
                _gridManager.Grid.GetTilesOwnedBy(playerId, output);
            }
            else
            {
                output.Clear();
            }
        }

        #endregion

        #region Terrain Queries

        /// <summary>
        /// Check if a tile is passable (units can traverse).
        /// </summary>
        public bool IsPassable(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.IsPassable ?? false;
        }

        /// <summary>
        /// Check if a tile is buildable.
        /// </summary>
        public bool IsBuildable(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.IsBuildable ?? false;
        }

        /// <summary>
        /// Get the movement cost for a tile.
        /// </summary>
        public float GetMovementCost(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.MovementCost ?? float.MaxValue;
        }

        /// <summary>
        /// Get the terrain type of a tile.
        /// </summary>
        public TerrainType GetTerrainType(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.TerrainType ?? TerrainType.Grassland;
        }

        /// <summary>
        /// Get the defense bonus for a tile.
        /// </summary>
        public int GetDefenseBonus(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.DefenseBonus ?? 0;
        }

        #endregion

        #region Resource Queries

        /// <summary>
        /// Get resources on a tile.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, TileResourceValue> GetResources(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.Resources;
        }

        /// <summary>
        /// Get the amount of a specific resource on a tile.
        /// </summary>
        public int GetResourceAmount(Vector2Int coordinates, ResourceType resourceType)
        {
            var tile = GetTile(coordinates);
            return tile?.GetResourceAmount(resourceType) ?? 0;
        }

        /// <summary>
        /// Check if a tile has any resources.
        /// </summary>
        public bool HasResources(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.HasResources ?? false;
        }

        #endregion

        #region Visibility Queries (Fog of War)

        /// <summary>
        /// Check if a tile is currently visible to a player.
        /// </summary>
        public bool IsVisibleToPlayer(Vector2Int coordinates, int playerId)
        {
            var tile = GetTile(coordinates);
            return tile?.IsVisibleToPlayer(playerId) ?? false;
        }

        /// <summary>
        /// Check if a tile has been explored by a player.
        /// </summary>
        public bool IsExploredByPlayer(Vector2Int coordinates, int playerId)
        {
            var tile = GetTile(coordinates);
            return tile?.IsExploredByPlayer(playerId) ?? false;
        }

        /// <summary>
        /// Get all tiles visible to a player.
        /// </summary>
        public List<HexTile> GetVisibleTiles(int playerId)
        {
            return _gridManager.Grid?.GetVisibleTiles(playerId) ?? new List<HexTile>();
        }

        /// <summary>
        /// Get all tiles visible to a player (non-allocating).
        /// </summary>
        public void GetVisibleTiles(int playerId, List<HexTile> output)
        {
            if (_gridManager.Grid != null)
            {
                _gridManager.Grid.GetVisibleTiles(playerId, output);
            }
            else
            {
                output.Clear();
            }
        }

        #endregion

        #region Building Queries

        /// <summary>
        /// Check if a tile is occupied by a building.
        /// </summary>
        public bool IsOccupied(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.IsOccupied ?? false;
        }

        /// <summary>
        /// Get the building occupying a tile.
        /// </summary>
        public GameObject GetOccupyingBuilding(Vector2Int coordinates)
        {
            var tile = GetTile(coordinates);
            return tile?.OccupyingBuilding;
        }

        #endregion
    }
}
