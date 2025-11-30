using RTS.Core.Time;
using UnityEngine;

namespace RTS.TurnBased
{
    /// <summary>
    /// Time provider for turn-based execution mode.
    /// Provides discrete time progression per turn instead of continuous time.
    /// </summary>
    public class TurnTimeProvider : ITimeProvider
    {
        private float _timeScale = 1f;
        private float _deltaTime;
        private float _totalTime;

        /// <summary>
        /// Time delta for this tick.
        /// In turn-based mode, this is set when a turn executes and reset to 0 after.
        /// </summary>
        public float DeltaTime => _deltaTime;

        /// <summary>
        /// In turn-based mode, time is effectively "paused" between turn executions.
        /// Returns true when not in the middle of processing a turn.
        /// </summary>
        public bool IsPaused => _deltaTime <= 0;

        /// <summary>
        /// Time scale multiplier (can speed up turn animations if needed).
        /// </summary>
        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Total elapsed game time (accumulated from all turns).
        /// </summary>
        public float TotalTime => _totalTime;

        /// <summary>
        /// Create a new turn-based time provider.
        /// </summary>
        public TurnTimeProvider()
        {
            _deltaTime = 0f;
            _totalTime = 0f;
            _timeScale = 1f;
        }

        /// <summary>
        /// Execute a turn tick with the specified delta time.
        /// This is called by TurnManager when processing turn-end logic.
        /// </summary>
        /// <param name="deltaTime">The time delta for this turn.</param>
        public void ExecuteTurnTick(float deltaTime)
        {
            _deltaTime = deltaTime * _timeScale;
            _totalTime += _deltaTime;
        }

        /// <summary>
        /// Reset delta time after turn processing is complete.
        /// </summary>
        public void ResetDeltaTime()
        {
            _deltaTime = 0f;
        }
    }
}
