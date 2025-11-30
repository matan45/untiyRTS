namespace RTS.TurnBased
{
    /// <summary>
    /// Interface for systems that need to respond to turn events.
    /// Implement this to receive notifications when turns start and end.
    /// </summary>
    public interface ITurnListener
    {
        /// <summary>
        /// Called when a new turn starts.
        /// Use this for turn-start logic like resource income.
        /// </summary>
        /// <param name="turnNumber">The turn number that just started.</param>
        void OnTurnStart(int turnNumber);

        /// <summary>
        /// Called when the current turn ends.
        /// Use this for turn-end logic like construction progress.
        /// </summary>
        /// <param name="turnNumber">The turn number that just ended.</param>
        void OnTurnEnd(int turnNumber);
    }
}
