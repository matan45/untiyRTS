using System.Collections.Generic;
using RTS.Data;

namespace RTS.Interfaces
{
    /// <summary>
    /// Interface for buildings that can produce units.
    /// Implements Interface Segregation Principle (ISP) - optional capability interface.
    /// Only production buildings (e.g., Barracks) need to implement this.
    /// </summary>
    public interface IUnitProducer
    {
        /// <summary>
        /// Checks if the building can currently produce the specified unit type.
        /// </summary>
        /// <param name="unitData">The unit data defining what to produce</param>
        /// <returns>True if production can start, false otherwise</returns>
        bool CanProduceUnit(UnitData unitData);

        /// <summary>
        /// Starts production of the specified unit.
        /// Adds the unit to the production queue.
        /// </summary>
        /// <param name="unitData">The unit data defining what to produce</param>
        void ProduceUnit(UnitData unitData);

        /// <summary>
        /// Gets the current production queue.
        /// </summary>
        /// <returns>Read-only list of units currently being produced</returns>
        IReadOnlyList<UnitData> GetProductionQueue();

        /// <summary>
        /// Cancels production at the specified queue index.
        /// </summary>
        /// <param name="queueIndex">Index in the production queue to cancel</param>
        void CancelProduction(int queueIndex);
    }
}
