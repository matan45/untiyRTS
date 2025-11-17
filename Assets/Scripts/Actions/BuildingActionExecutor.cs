using RTS.Buildings;
using RTS.Interfaces;
using RTS.Selection;
using UnityEngine;

namespace RTS.Actions
{
    /// <summary>
    /// Singleton service that executes building actions (Sell, Upgrade, Produce Unit, etc.).
    /// Implements Single Responsibility Principle (SRP) - centralized action execution logic.
    /// Implements Command Pattern - actions are executed through a central executor.
    /// </summary>
    public class BuildingActionExecutor : MonoBehaviour
    {
        public static BuildingActionExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Executes the specified action on the given building.
        /// </summary>
        /// <param name="building">The building to execute the action on</param>
        /// <param name="actionId">The action identifier</param>
        public void ExecuteAction(Building building, string actionId)
        {
            if (building == null || string.IsNullOrEmpty(actionId))
            {
                return;
            }


            switch (actionId)
            {
                case "sell":
                    ExecuteSell(building);
                    break;

                case "upgrade":
                    ExecuteUpgrade(building);
                    break;

                case "repair":
                    ExecuteRepair(building);
                    break;

                default:
                    // Check if it's a unit production action
                    if (actionId.StartsWith("produce_"))
                    {
                        ExecuteProduceUnit(building, actionId);
                    }
                    break;
            }
        }

        #region Action Implementations

        private void ExecuteSell(Building building)
        {
            if (!building.IsConstructed)
            {
                return;
            }

            // Calculate refund (50% of original cost)
            int refund = Mathf.FloorToInt(building.Data.creditsCost * 0.5f);

            // Refund resources
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.RefundResources(refund, 0);
            }

            // Deselect if selected
            if (building.IsSelected && BuildingSelectionManager.Instance != null)
            {
                BuildingSelectionManager.Instance.ClearSelection();
            }

            // Destroy the building
            Destroy(building.GameObject);
        }

        private void ExecuteUpgrade(Building building)
        {
            if (building is IUpgradeable upgradeable)
            {
                if (upgradeable.CanUpgrade())
                {
                    upgradeable.StartUpgrade();
                }
               
            }
           
        }

        private void ExecuteRepair(Building building)
        {
            if (building is IRepairable repairable)
            {
                repairable.Repair();
            }
           
        }

        private void ExecuteProduceUnit(Building building, string actionId)
        {
            if (building is IUnitProducer producer)
            {
                // Extract unit type from actionId (e.g., "produce_infantry" -> "infantry")
                string unitType = actionId.Substring("produce_".Length);

                // In a real implementation, you'd load the UnitData based on unitType
                // For now, we'll just log it

                // TODO: Load UnitData asset and call producer.ProduceUnit(unitData)
            }
           
        }

        #endregion
    }
}
