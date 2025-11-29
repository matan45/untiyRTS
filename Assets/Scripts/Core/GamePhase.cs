namespace RTS.Core
{
    /// <summary>
    /// Represents the current phase of the game.
    /// Used for coordinating system updates and UI state.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// Game is initializing (loading map, setting up systems).
        /// </summary>
        Initialization,

        /// <summary>
        /// Game is actively being played.
        /// </summary>
        Playing,

        /// <summary>
        /// Game is paused (menu open, etc.).
        /// </summary>
        Paused,

        /// <summary>
        /// Game has ended (victory or defeat).
        /// </summary>
        GameOver
    }
}
