using RTS.Data;

namespace RTS.Interfaces
{
    /// <summary>
    /// Interface for buildings that can perform actions (sell, upgrade, produce units, etc.).
    /// Implements Interface Segregation Principle (ISP) - focused on action capabilities.
    /// </summary>
    public interface IBuildingActions
    {
        /// <summary>
        /// Gets the action configuration for this building.
        /// Returns null if no actions are available.
        /// </summary>
        BuildingActionConfig GetActionConfig();

        /// <summary>
        /// Checks if the building can currently execute the specified action.
        /// </summary>
        /// <param name="actionId">Unique identifier for the action (e.g., "sell", "upgrade", "produce_unit")</param>
        /// <returns>True if the action can be executed, false otherwise</returns>
        bool CanExecuteAction(string actionId);

        /// <summary>
        /// Executes the specified action.
        /// </summary>
        /// <param name="actionId">Unique identifier for the action</param>
        void ExecuteAction(string actionId);
    }
}
