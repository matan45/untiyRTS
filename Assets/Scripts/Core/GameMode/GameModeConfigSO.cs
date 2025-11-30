using UnityEngine;

namespace RTS.Core.GameMode
{
    /// <summary>
    /// Configuration for game mode settings.
    /// Assign this to GameModeManager to configure the game mode at startup.
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "RTS/Game Mode Config")]
    public class GameModeConfigSO : ScriptableObject
    {
        /// <summary>
        /// The type of game mode to use.
        /// </summary>
        public enum GameModeType
        {
            RealTime,
            TurnBased
        }

        [Header("Mode Selection")]
        [Tooltip("The game mode to use. Set at initialization, cannot be changed during gameplay.")]
        public GameModeType gameMode = GameModeType.RealTime;

        [Header("Real-Time Settings")]
        [Tooltip("Initial time scale for real-time mode.")]
        [Range(0.1f, 3f)]
        public float initialTimeScale = 1f;

        [Header("Turn-Based Settings")]
        [Tooltip("Whether to auto-advance turns after player ends their turn.")]
        public bool autoAdvanceTurns = false;

        [Tooltip("Delay between turns in seconds (for visual feedback).")]
        [Range(0f, 2f)]
        public float turnTransitionDelay = 0.5f;
    }
}
