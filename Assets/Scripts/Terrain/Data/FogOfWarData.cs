using System;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// Serializable data class for per-player visibility state on a tile.
    /// Used for Fog of War system.
    /// </summary>
    [Serializable]
    public class PlayerVisibilityState
    {
        /// <summary>
        /// Player ID this visibility state belongs to.
        /// </summary>
        public int playerId;

        /// <summary>
        /// Whether the tile is currently visible to this player.
        /// True = units/buildings providing vision, False = not currently visible.
        /// </summary>
        public bool isVisible;

        /// <summary>
        /// Whether the tile has ever been explored by this player.
        /// True = seen at least once (fog), False = never seen (black).
        /// </summary>
        public bool hasBeenExplored;

        /// <summary>
        /// Create a default visibility state (visible and explored).
        /// </summary>
        public PlayerVisibilityState()
        {
            playerId = 0;
            isVisible = true;
            hasBeenExplored = true;
        }

        /// <summary>
        /// Create a visibility state for a specific player.
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="visible">Initial visibility state</param>
        /// <param name="explored">Initial exploration state</param>
        public PlayerVisibilityState(int playerId, bool visible = true, bool explored = true)
        {
            this.playerId = playerId;
            this.isVisible = visible;
            this.hasBeenExplored = explored;
        }

        /// <summary>
        /// Create a copy of this visibility state.
        /// </summary>
        public PlayerVisibilityState Clone()
        {
            return new PlayerVisibilityState(playerId, isVisible, hasBeenExplored);
        }
    }
}
