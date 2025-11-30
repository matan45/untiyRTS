using RTS.Core.Time;

namespace RTS.Core.GameMode
{
    /// <summary>
    /// Strategy interface for different game execution modes.
    /// Implementations handle the specific timing and flow for each mode.
    /// </summary>
    public interface IGameMode
    {
        /// <summary>
        /// Display name of this game mode.
        /// </summary>
        string ModeName { get; }

        /// <summary>
        /// The time provider for this mode.
        /// </summary>
        ITimeProvider TimeProvider { get; }

        /// <summary>
        /// Whether the player can currently perform actions.
        /// In real-time: typically always true during Playing phase.
        /// In turn-based: true during the player's turn.
        /// </summary>
        bool CanPlayerAct { get; }

        /// <summary>
        /// Initialize the game mode.
        /// Called when this mode becomes active.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Clean up when switching away from this mode.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Called each Unity Update frame.
        /// Handle mode-specific per-frame logic here.
        /// </summary>
        void Update();

        /// <summary>
        /// Handle game phase changes (Playing, Paused, etc.).
        /// </summary>
        /// <param name="phase">The new game phase.</param>
        void OnPhaseChanged(GamePhase phase);
    }
}
