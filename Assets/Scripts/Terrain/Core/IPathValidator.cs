using UnityEngine;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Interface for validating pathfinding operations on the hex grid.
    /// Different unit types can have different validators (ground, air, naval).
    /// Implements Interface Segregation Principle (ISP) - focused interface for pathfinding needs.
    /// </summary>
    public interface IPathValidator
    {
        /// <summary>
        /// Check if a tile can be traversed.
        /// </summary>
        /// <param name="coordinates">The tile coordinates to check</param>
        /// <returns>True if the tile is passable</returns>
        bool IsPassable(Vector2Int coordinates);

        /// <summary>
        /// Get the movement cost to enter a tile.
        /// Used for A* pathfinding cost calculation.
        /// </summary>
        /// <param name="coordinates">The destination tile coordinates</param>
        /// <returns>Movement cost (higher = slower). Return float.MaxValue for impassable.</returns>
        float GetMovementCost(Vector2Int coordinates);

        /// <summary>
        /// Validate if a direct move between two tiles is valid.
        /// Checks adjacency, passability, and any blocking conditions.
        /// </summary>
        /// <param name="from">Starting tile coordinates</param>
        /// <param name="to">Destination tile coordinates</param>
        /// <returns>True if the move is valid</returns>
        bool IsValidMove(Vector2Int from, Vector2Int to);
    }
}
