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
    }
}
