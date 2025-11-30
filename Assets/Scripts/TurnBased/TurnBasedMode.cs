using RTS.Core;
using RTS.Core.GameMode;
using RTS.Core.Time;
using RTS.Core.Ticking;
using UnityEngine;

namespace RTS.TurnBased
{
    /// <summary>
    /// Turn-based game mode implementation.
    /// Provides discrete turn progression with start/end events.
    /// </summary>
    public class TurnBasedMode : IGameMode
    {
        private TurnTimeProvider _timeProvider;
        private TurnManager _turnManager;
        private GameModeConfigSO _config;
        private bool _canPlayerAct;
        private bool _isGameActive;

        /// <summary>
        /// Display name of this game mode.
        /// </summary>
        public string ModeName => "Turn-Based";

        /// <summary>
        /// The time provider for turn-based mode.
        /// </summary>
        public ITimeProvider TimeProvider => _timeProvider;

        /// <summary>
        /// Whether the player can currently perform actions.
        /// In turn-based mode, true during the player's turn.
        /// </summary>
        public bool CanPlayerAct => _canPlayerAct && _isGameActive;

        /// <summary>
        /// The turn manager for this mode.
        /// </summary>
        public TurnManager TurnManager => _turnManager;

        /// <summary>
        /// Current turn number.
        /// </summary>
        public int CurrentTurn => _turnManager?.CurrentTurn ?? 0;

        /// <summary>
        /// Create a new turn-based mode with optional configuration.
        /// </summary>
        /// <param name="config">Configuration settings (optional).</param>
        public TurnBasedMode(GameModeConfigSO config = null)
        {
            _config = config;
            _timeProvider = new TurnTimeProvider();
            _turnManager = new TurnManager();
            _canPlayerAct = false;
            _isGameActive = false;
        }

        /// <summary>
        /// Initialize the turn-based mode.
        /// </summary>
        public void Initialize()
        {
            _canPlayerAct = false;
            _isGameActive = false;

            // Subscribe to turn events to manage time provider
            _turnManager.OnTurnEnd += HandleTurnEnd;

            Debug.Log("TurnBasedMode: Initialized");
        }

        /// <summary>
        /// Shutdown the turn-based mode.
        /// </summary>
        public void Shutdown()
        {
            _canPlayerAct = false;
            _isGameActive = false;

            _turnManager.OnTurnEnd -= HandleTurnEnd;

            Debug.Log("TurnBasedMode: Shutdown");
        }

        /// <summary>
        /// Called each Unity Update frame.
        /// In turn-based mode, most logic happens in response to turn events.
        /// </summary>
        public void Update()
        {
            // Reset delta time after tick processing is complete
            // This ensures IsPaused returns true between turns
            _timeProvider.ResetDeltaTime();
        }

        /// <summary>
        /// Handle game phase changes.
        /// </summary>
        /// <param name="phase">The new game phase.</param>
        public void OnPhaseChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Playing:
                    _isGameActive = true;
                    _canPlayerAct = true;
                    // Start first turn when game begins
                    if (_turnManager.CurrentTurn == 0)
                    {
                        _turnManager.StartTurn();
                    }
                    break;

                case GamePhase.Paused:
                    _canPlayerAct = false;
                    break;

                case GamePhase.GameOver:
                    _isGameActive = false;
                    _canPlayerAct = false;
                    break;

                case GamePhase.Initialization:
                    _isGameActive = false;
                    _canPlayerAct = false;
                    break;
            }

            Debug.Log($"TurnBasedMode: Phase changed to {phase}, CanPlayerAct={CanPlayerAct}");
        }

        /// <summary>
        /// End the current turn and start the next one.
        /// Call this when the player clicks "End Turn".
        /// </summary>
        public void EndPlayerTurn()
        {
            if (!CanPlayerAct)
            {
                Debug.LogWarning("TurnBasedMode: Cannot end turn - player cannot act.");
                return;
            }

            _canPlayerAct = false;
            _turnManager.EndTurn();

            // Auto-start next turn (player's turn again since no AI yet)
            _turnManager.StartTurn();
            _canPlayerAct = true;
        }

        /// <summary>
        /// Handle turn end to trigger tick processing.
        /// </summary>
        private void HandleTurnEnd(int turnNumber)
        {
            // Provide a time tick for systems that need it
            // This allows ITickable systems to process once per turn
            _timeProvider.ExecuteTurnTick(1f); // 1 unit of time per turn

            // Process all tickables with this turn's time
            if (TickManager.Instance != null)
            {
                TickManager.Instance.ProcessTick(_timeProvider.DeltaTime);
            }
        }
    }
}
