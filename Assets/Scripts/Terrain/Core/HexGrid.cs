using UnityEngine;
using System.Collections.Generic;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Manages the hexagonal grid data structure.
    /// Stores tiles and provides access methods.
    /// </summary>
    public class HexGrid
    {
        private Dictionary<Vector2Int, HexTile> tiles;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public HexGrid(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new Dictionary<Vector2Int, HexTile>();
        }

        /// <summary>
        /// Add a tile to the grid.
        /// </summary>
        public void AddTile(HexTile tile)
        {
            tiles[tile.Coordinates] = tile;
        }

        /// <summary>
        /// Get a tile at the specified coordinates.
        /// </summary>
        public HexTile GetTile(Vector2Int coordinates)
        {
            tiles.TryGetValue(coordinates, out HexTile tile);
            return tile;
        }

        /// <summary>
        /// Get a tile at the specified coordinates.
        /// </summary>
        public HexTile GetTile(int q, int r)
        {
            return GetTile(new Vector2Int(q, r));
        }

        /// <summary>
        /// Check if coordinates are within grid bounds.
        /// </summary>
        public bool IsValidCoordinate(Vector2Int coordinates)
        {
            return tiles.ContainsKey(coordinates);
        }

        /// <summary>
        /// Get all tiles in the grid.
        /// </summary>
        public IEnumerable<HexTile> GetAllTiles()
        {
            return tiles.Values;
        }

        // Shared buffer for non-allocating neighbor coordinate lookups
        [System.ThreadStatic]
        private static Vector2Int[] _neighborCoordsBuffer;

        private static Vector2Int[] NeighborCoordsBuffer
        {
            get
            {
                _neighborCoordsBuffer ??= new Vector2Int[6];
                return _neighborCoordsBuffer;
            }
        }

        /// <summary>
        /// Get all valid neighboring tiles for a given coordinate.
        /// Only returns tiles that exist in the grid.
        /// Note: Allocates a new list. For performance-critical code, use the non-allocating overload.
        /// </summary>
        /// <param name="coordinates">The center hex coordinate</param>
        /// <returns>List of existing neighbor tiles (0-6 tiles)</returns>
        public List<HexTile> GetNeighbors(Vector2Int coordinates)
        {
            List<HexTile> neighbors = new List<HexTile>(6);
            GetNeighbors(coordinates, neighbors);
            return neighbors;
        }

        /// <summary>
        /// Get all valid neighboring tiles for a given coordinate (non-allocating).
        /// Use this overload in performance-critical code like pathfinding.
        /// </summary>
        /// <param name="coordinates">The center hex coordinate</param>
        /// <param name="output">Pre-allocated list to store results (will be cleared first)</param>
        public void GetNeighbors(Vector2Int coordinates, List<HexTile> output)
        {
            output.Clear();
            HexCoordinates.GetNeighborCoordinates(coordinates, NeighborCoordsBuffer);

            for (int i = 0; i < 6; i++)
            {
                HexTile tile = GetTile(NeighborCoordsBuffer[i]);
                if (tile != null)
                {
                    output.Add(tile);
                }
            }
        }

        /// <summary>
        /// Get all valid neighboring tiles for a given tile.
        /// Convenience overload that extracts coordinates from tile.
        /// Note: Allocates a new list. For performance-critical code, use the non-allocating overload.
        /// </summary>
        /// <param name="tile">The center tile</param>
        /// <returns>List of existing neighbor tiles (0-6 tiles)</returns>
        public List<HexTile> GetNeighbors(HexTile tile)
        {
            if (tile == null) return new List<HexTile>();
            return GetNeighbors(tile.Coordinates);
        }

        /// <summary>
        /// Get all valid neighboring tiles for a given tile (non-allocating).
        /// Use this overload in performance-critical code like pathfinding.
        /// </summary>
        /// <param name="tile">The center tile</param>
        /// <param name="output">Pre-allocated list to store results (will be cleared first)</param>
        public void GetNeighbors(HexTile tile, List<HexTile> output)
        {
            if (tile == null)
            {
                output.Clear();
                return;
            }
            GetNeighbors(tile.Coordinates, output);
        }

        #region Fog of War / Visibility Methods

        /// <summary>
        /// Initialize visibility for all tiles for all players.
        /// Call this after grid generation to set up per-player visibility states.
        /// </summary>
        /// <param name="playerCount">Number of players to initialize visibility for</param>
        /// <param name="defaultVisible">Default visibility state (true = all visible, for RTS-19)</param>
        /// <param name="defaultExplored">Default exploration state (true = all explored, for RTS-19)</param>
        public void InitializeVisibilityForAllTiles(int playerCount, bool defaultVisible = true, bool defaultExplored = true)
        {
            foreach (var tile in tiles.Values)
            {
                for (int playerId = 0; playerId < playerCount; playerId++)
                {
                    tile.InitializeVisibilityForPlayer(playerId, defaultVisible, defaultExplored);
                }
            }
        }

        /// <summary>
        /// Get all tiles visible to a specific player.
        /// Note: Allocates a new list. For performance-critical code, use the non-allocating overload.
        /// </summary>
        /// <param name="playerId">Player ID to check visibility for</param>
        /// <returns>List of visible tiles</returns>
        public List<HexTile> GetVisibleTiles(int playerId)
        {
            var result = new List<HexTile>();
            GetVisibleTiles(playerId, result);
            return result;
        }

        /// <summary>
        /// Get all tiles visible to a specific player (non-allocating).
        /// Use this overload in performance-critical code like per-frame updates.
        /// </summary>
        /// <param name="playerId">Player ID to check visibility for</param>
        /// <param name="output">Pre-allocated list to store results (will be cleared first)</param>
        public void GetVisibleTiles(int playerId, List<HexTile> output)
        {
            output.Clear();
            foreach (var tile in tiles.Values)
            {
                if (tile.IsVisibleToPlayer(playerId))
                {
                    output.Add(tile);
                }
            }
        }

        /// <summary>
        /// Get all tiles explored by a specific player.
        /// Note: Allocates a new list. For performance-critical code, use the non-allocating overload.
        /// </summary>
        /// <param name="playerId">Player ID to check exploration for</param>
        /// <returns>List of explored tiles</returns>
        public List<HexTile> GetExploredTiles(int playerId)
        {
            var result = new List<HexTile>();
            GetExploredTiles(playerId, result);
            return result;
        }

        /// <summary>
        /// Get all tiles explored by a specific player (non-allocating).
        /// Use this overload in performance-critical code like per-frame updates.
        /// </summary>
        /// <param name="playerId">Player ID to check exploration for</param>
        /// <param name="output">Pre-allocated list to store results (will be cleared first)</param>
        public void GetExploredTiles(int playerId, List<HexTile> output)
        {
            output.Clear();
            foreach (var tile in tiles.Values)
            {
                if (tile.IsExploredByPlayer(playerId))
                {
                    output.Add(tile);
                }
            }
        }

        /// <summary>
        /// Get all tiles not yet explored by a specific player (black fog).
        /// Note: Allocates a new list. For performance-critical code, use the non-allocating overload.
        /// </summary>
        /// <param name="playerId">Player ID to check</param>
        /// <returns>List of unexplored tiles</returns>
        public List<HexTile> GetUnexploredTiles(int playerId)
        {
            var result = new List<HexTile>();
            GetUnexploredTiles(playerId, result);
            return result;
        }

        /// <summary>
        /// Get all tiles not yet explored by a specific player (non-allocating).
        /// Use this overload in performance-critical code like per-frame updates.
        /// </summary>
        /// <param name="playerId">Player ID to check</param>
        /// <param name="output">Pre-allocated list to store results (will be cleared first)</param>
        public void GetUnexploredTiles(int playerId, List<HexTile> output)
        {
            output.Clear();
            foreach (var tile in tiles.Values)
            {
                if (!tile.IsExploredByPlayer(playerId))
                {
                    output.Add(tile);
                }
            }
        }

        /// <summary>
        /// Set visibility for multiple tiles at once (batch operation).
        /// Useful for updating vision around units.
        /// </summary>
        /// <param name="coordinates">List of tile coordinates to update</param>
        /// <param name="playerId">Player ID to set visibility for</param>
        /// <param name="isVisible">Visibility state to set</param>
        public void SetVisibilityBatch(IEnumerable<Vector2Int> coordinates, int playerId, bool isVisible)
        {
            foreach (var coord in coordinates)
            {
                var tile = GetTile(coord);
                if (tile != null)
                {
                    tile.SetVisibility(playerId, isVisible);
                }
            }
        }

        /// <summary>
        /// Get all tiles owned by a specific player.
        /// Note: Allocates a new list. For performance-critical code, use the non-allocating overload.
        /// </summary>
        /// <param name="playerId">Player ID to check ownership for</param>
        /// <returns>List of tiles owned by the player</returns>
        public List<HexTile> GetTilesOwnedBy(int playerId)
        {
            var result = new List<HexTile>();
            GetTilesOwnedBy(playerId, result);
            return result;
        }

        /// <summary>
        /// Get all tiles owned by a specific player (non-allocating).
        /// Use this overload in performance-critical code.
        /// </summary>
        /// <param name="playerId">Player ID to check ownership for</param>
        /// <param name="output">Pre-allocated list to store results (will be cleared first)</param>
        public void GetTilesOwnedBy(int playerId, List<HexTile> output)
        {
            output.Clear();
            foreach (var tile in tiles.Values)
            {
                if (tile.IsOwnedBy(playerId))
                {
                    output.Add(tile);
                }
            }
        }

        #endregion
    }
}
