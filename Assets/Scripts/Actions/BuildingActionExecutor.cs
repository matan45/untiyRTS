using UnityEngine;
using RTS.Buildings;
using RTS.Interfaces;
using RTS.Selection;

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
                Debug.LogWarning("BuildingActionExecutor: Invalid building or actionId");
                return;
            }

            Debug.Log($"Executing action '{actionId}' on {building.GameObject.name}");

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
                    else
                    {
                        Debug.LogWarning($"Unknown action ID: {actionId}");
                    }
                    break;
            }
        }

        #region Action Implementations

        private void ExecuteSell(Building building)
        {
            if (!building.IsConstructed)
            {
                Debug.LogWarning("Cannot sell building that is not yet constructed");
                return;
            }

            // Calculate refund (50% of original cost)
            int refund = Mathf.FloorToInt(building.Data.creditsCost * 0.5f);

            // Refund resources
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.RefundResources(refund, 0);
                Debug.Log($"Sold {building.GameObject.name} for {refund} credits");
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
                    Debug.Log($"Started upgrade for {building.GameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"Cannot upgrade {building.GameObject.name} - requirements not met");
                }
            }
            else
            {
                Debug.LogWarning($"{building.GameObject.name} does not implement IUpgradeable");
            }
        }

        private void ExecuteRepair(Building building)
        {
            if (building is IRepairable repairable)
            {
                repairable.Repair();
                Debug.Log($"Repaired {building.GameObject.name}");
            }
            else
            {
                Debug.LogWarning($"{building.GameObject.name} does not implement IRepairable");
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
                Debug.Log($"Producing unit of type '{unitType}' from {building.GameObject.name}");

                // TODO: Load UnitData asset and call producer.ProduceUnit(unitData)
                Debug.LogWarning("Unit production not yet fully implemented - needs UnitData asset loading");
            }
            else
            {
                Debug.LogWarning($"{building.GameObject.name} does not implement IUnitProducer");
            }
        }

        #endregion
    }
}
