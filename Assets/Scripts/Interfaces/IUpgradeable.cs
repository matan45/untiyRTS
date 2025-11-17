using RTS.Data;

namespace RTS.Interfaces
{
    /// <summary>
    /// Interface for buildings that can be upgraded to more advanced versions.
    /// Implements Interface Segregation Principle (ISP) - optional capability interface.
    /// Only upgradeable buildings need to implement this.
    /// </summary>
    public interface IUpgradeable
    {
        /// <summary>
        /// Gets the BuildingData for the upgraded version of this building.
        /// Returns null if no upgrade is available.
        /// </summary>
        BuildingData GetUpgradeData();

        /// <summary>
        /// Checks if this building can currently be upgraded.
        /// Considers resources, prerequisites, and upgrade availability.
        /// </summary>
        /// <returns>True if upgrade is available and affordable, false otherwise</returns>
        bool CanUpgrade();

        /// <summary>
        /// Starts the upgrade process for this building.
        /// </summary>
        void StartUpgrade();

        /// <summary>
        /// Gets the progress of the current upgrade (0-1).
        /// Returns 0 if not currently upgrading.
        /// </summary>
        float GetUpgradeProgress();
    }
}
