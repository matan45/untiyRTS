namespace RTS.Core.Time
{
    /// <summary>
    /// Interface for the time management system.
    /// Allows mocking for unit tests while maintaining singleton access.
    /// </summary>
    public interface ITimeManager
    {
        /// <summary>
        /// The current active time provider.
        /// </summary>
        ITimeProvider CurrentProvider { get; }

        /// <summary>
        /// Convenience property to get delta time from current provider.
        /// Returns 0 if no provider is set.
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Convenience property to check if time is paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Convenience property for total elapsed time.
        /// </summary>
        float TotalTime { get; }

        /// <summary>
        /// Set the current time provider.
        /// Called by GameModeManager when mode is initialized.
        /// </summary>
        /// <param name="provider">The time provider to use.</param>
        void SetProvider(ITimeProvider provider);
    }
}
