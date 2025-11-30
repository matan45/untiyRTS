namespace RTS.Core.Ticking
{
    /// <summary>
    /// Interface for systems that need time-based updates.
    /// Implement this instead of using Update() directly to support both
    /// real-time and turn-based execution modes.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// Called each tick with the time delta.
        /// In real-time: called every frame with Time.deltaTime
        /// In turn-based: called once per turn with turn's time value
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tick.</param>
        void Tick(float deltaTime);

        /// <summary>
        /// Priority for tick order. Lower values tick first.
        /// Suggested ranges:
        /// - 0-99: High priority (resource generation, etc.)
        /// - 100-199: Normal priority (construction, production)
        /// - 200+: Low priority (cleanup, UI updates)
        /// </summary>
        int TickPriority { get; }

        /// <summary>
        /// Whether this tickable should currently receive ticks.
        /// Return false to skip processing (e.g., when queue is empty).
        /// </summary>
        bool IsTickActive { get; }
    }
}
