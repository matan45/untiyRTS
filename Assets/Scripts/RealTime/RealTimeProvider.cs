using RTS.Core.Time;
using UnityEngine;

namespace RTS.RealTime
{
    /// <summary>
    /// Time provider for real-time execution mode.
    /// Uses Unity's Time.deltaTime for continuous time progression.
    /// </summary>
    public class RealTimeProvider : ITimeProvider
    {
        private float _timeScale = 1f;
        private bool _isPaused;
        private float _totalTime;

        /// <summary>
        /// Time delta for this frame.
        /// Returns Time.deltaTime * TimeScale when not paused, 0 when paused.
        /// </summary>
        public float DeltaTime
        {
            get
            {
                if (_isPaused) return 0f;
                return UnityEngine.Time.deltaTime * _timeScale;
            }
        }

        /// <summary>
        /// Whether time is currently paused.
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }

        /// <summary>
        /// Time scale multiplier for speed controls.
        /// </summary>
        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Total elapsed game time (only counts unpaused time).
        /// </summary>
        public float TotalTime => _totalTime;

        /// <summary>
        /// Create a new real-time provider with optional initial time scale.
        /// </summary>
        /// <param name="initialTimeScale">Initial time scale (default 1.0).</param>
        public RealTimeProvider(float initialTimeScale = 1f)
        {
            _timeScale = Mathf.Max(0f, initialTimeScale);
            _isPaused = false;
            _totalTime = 0f;
        }

        /// <summary>
        /// Update total time. Called each frame by RealTimeMode.
        /// </summary>
        public void UpdateTotalTime()
        {
            if (!_isPaused)
            {
                _totalTime += DeltaTime;
            }
        }
    }
}
