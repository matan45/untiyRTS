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

        /// <summary>
        /// Get all valid neighboring tiles for a given coordinate.
        /// Only returns tiles that exist in the grid.
        /// </summary>
        /// <param name="coordinates">The center hex coordinate</param>
        /// <returns>List of existing neighbor tiles (0-6 tiles)</returns>
        public List<HexTile> GetNeighbors(Vector2Int coordinates)
        {
            List<HexTile> neighbors = new List<HexTile>(6);
            Vector2Int[] neighborCoords = HexCoordinates.GetNeighborCoordinates(coordinates);

            foreach (Vector2Int coord in neighborCoords)
            {
                HexTile tile = GetTile(coord);
                if (tile != null)
                {
                    neighbors.Add(tile);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Get all valid neighboring tiles for a given tile.
        /// Convenience overload that extracts coordinates from tile.
        /// </summary>
        /// <param name="tile">The center tile</param>
        /// <returns>List of existing neighbor tiles (0-6 tiles)</returns>
        public List<HexTile> GetNeighbors(HexTile tile)
        {
            if (tile == null) return new List<HexTile>();
            return GetNeighbors(tile.Coordinates);
        }
    }
}
