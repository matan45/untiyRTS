namespace RTS.Core.Time
{
    /// <summary>
    /// Abstracts time progression for game systems.
    /// Different implementations handle real-time vs turn-based timing.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Time delta for this frame/tick.
        /// In real-time: Time.deltaTime * TimeScale
        /// In turn-based: Fixed amount per tick (or 0 between turns)
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Whether time progression is currently paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Time scale multiplier for speed controls.
        /// </summary>
        float TimeScale { get; set; }

        /// <summary>
        /// Total elapsed game time.
        /// </summary>
        float TotalTime { get; }
    }
}
