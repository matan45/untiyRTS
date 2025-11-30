using RTS.Core;
using RTS.Core.GameMode;
using RTS.Core.Time;

namespace RTS.RealTime
{
    /// <summary>
    /// Real-time game mode implementation.
    /// Provides continuous time progression using Unity's Time.deltaTime.
    /// </summary>
    public class RealTimeMode : IGameMode
    {
        private RealTimeProvider _timeProvider;
        private bool _canPlayerAct;

        /// <summary>
        /// Display name of this game mode.
        /// </summary>
        public string ModeName => "Real-Time";

        /// <summary>
        /// The time provider for real-time mode.
        /// </summary>
        public ITimeProvider TimeProvider => _timeProvider;

        /// <summary>
        /// Whether the player can currently perform actions.
        /// In real-time mode, always true during Playing phase.
        /// </summary>
        public bool CanPlayerAct => _canPlayerAct;

        /// <summary>
        /// Create a new real-time mode with optional initial time scale.
        /// </summary>
        /// <param name="initialTimeScale">Initial time scale (default 1.0).</param>
        public RealTimeMode(float initialTimeScale = 1f)
        {
            _timeProvider = new RealTimeProvider(initialTimeScale);
            _canPlayerAct = false; // Set to true when game phase is Playing
        }

        /// <summary>
        /// Initialize the real-time mode.
        /// </summary>
        public void Initialize()
        {
            _canPlayerAct = false;
            _timeProvider.IsPaused = true; // Start paused until game phase is Playing

            UnityEngine.Debug.Log("RealTimeMode: Initialized");
        }

        /// <summary>
        /// Shutdown the real-time mode.
        /// </summary>
        public void Shutdown()
        {
            _canPlayerAct = false;
            _timeProvider.IsPaused = true;

            UnityEngine.Debug.Log("RealTimeMode: Shutdown");
        }

        /// <summary>
        /// Called each Unity Update frame.
        /// Updates total time tracking.
        /// </summary>
        public void Update()
        {
            _timeProvider.UpdateTotalTime();
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
                    _timeProvider.IsPaused = false;
                    _canPlayerAct = true;
                    break;

                case GamePhase.Paused:
                    _timeProvider.IsPaused = true;
                    _canPlayerAct = false;
                    break;

                case GamePhase.GameOver:
                    _timeProvider.IsPaused = true;
                    _canPlayerAct = false;
                    break;

                case GamePhase.Initialization:
                    _timeProvider.IsPaused = true;
                    _canPlayerAct = false;
                    break;
            }

            UnityEngine.Debug.Log($"RealTimeMode: Phase changed to {phase}, CanPlayerAct={_canPlayerAct}");
        }

        /// <summary>
        /// Set the time scale for speed controls.
        /// </summary>
        /// <param name="scale">New time scale (0.1 to 3.0 recommended).</param>
        public void SetTimeScale(float scale)
        {
            _timeProvider.TimeScale = scale;
        }
    }
}
