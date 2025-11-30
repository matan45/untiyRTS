using RTS.TurnBased;

namespace RTS.Core.GameMode
{
    /// <summary>
    /// Interface for the game mode management system.
    /// Allows mocking for unit tests while maintaining singleton access.
    /// </summary>
    public interface IGameModeManager
    {
        /// <summary>
        /// The currently active game mode.
        /// </summary>
        IGameMode CurrentMode { get; }

        /// <summary>
        /// Whether the current mode is Real-Time.
        /// </summary>
        bool IsRealTime { get; }

        /// <summary>
        /// Whether the current mode is Turn-Based.
        /// </summary>
        bool IsTurnBased { get; }

        /// <summary>
        /// Whether the player can currently perform actions.
        /// </summary>
        bool CanPlayerAct { get; }

        /// <summary>
        /// The game mode configuration.
        /// </summary>
        GameModeConfigSO Config { get; }

        /// <summary>
        /// Get the turn manager if in turn-based mode.
        /// Returns null if in real-time mode.
        /// </summary>
        TurnManager GetTurnManager();
    }
}
