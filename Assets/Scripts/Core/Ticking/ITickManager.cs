namespace RTS.Core.Ticking
{
    /// <summary>
    /// Interface for the tick management system.
    /// Allows mocking for unit tests while maintaining singleton access.
    /// </summary>
    public interface ITickManager
    {
        /// <summary>
        /// Number of registered tickable systems.
        /// </summary>
        int TickableCount { get; }

        /// <summary>
        /// Register a tickable system to receive time updates.
        /// </summary>
        /// <param name="tickable">The system to register.</param>
        void Register(ITickable tickable);

        /// <summary>
        /// Unregister a tickable system from receiving time updates.
        /// </summary>
        /// <param name="tickable">The system to unregister.</param>
        void Unregister(ITickable tickable);

        /// <summary>
        /// Process all active tickable systems with the given delta time.
        /// </summary>
        /// <param name="deltaTime">Time delta to pass to systems.</param>
        void ProcessTick(float deltaTime);
    }
}
