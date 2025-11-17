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

            // Validate building data exists (per CLAUDE.md defensive programming guidelines)
            if (building.Data == null)
            {
                Debug.LogWarning($"Cannot sell building {building.GameObject.name}: BuildingData is null");
                return;
            }

            // Validate ResourceManager exists before refunding
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("BuildingActionExecutor.ExecuteSell: ResourceManager.Instance is null!");
                return;
            }

            // Calculate refund (50% of original cost)
            int refund = Mathf.FloorToInt(building.Data.creditsCost * 0.5f);

            // Refund resources
            ResourceManager.Instance.RefundResources(refund, 0);

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
            // TODO: Add resource cost validation before upgrade
            // TODO: Show upgrade progress UI
            // TODO: Handle upgrade completion and replace building prefab
            if (building is IUpgradeable upgradeable)
            {
                if (upgradeable.CanUpgrade())
                {
                    upgradeable.StartUpgrade();
                }
                else
                {
                    Debug.LogWarning($"Building {building.GameObject.name} cannot be upgraded at this time");
                }
            }
            else
            {
                Debug.LogWarning($"Building {building.GameObject.name} does not implement IUpgradeable");
            }
        }

        private void ExecuteRepair(Building building)
        {
            // TODO: Add resource cost for repair (e.g., credits based on damage)
            // TODO: Validate player has enough resources
            // TODO: Show repair progress visual feedback
            if (building is IRepairable repairable)
            {
                repairable.Repair();
            }
            else
            {
                Debug.LogWarning($"Building {building.GameObject.name} does not implement IRepairable");
            }
        }

        private void ExecuteProduceUnit(Building building, string actionId)
        {
            if (building is IUnitProducer producer)
            {
                // Extract unit type from actionId (e.g., "produce_infantry" -> "infantry")
                string unitType = actionId.Substring("produce_".Length);

                // TODO: Implement unit production system
                // - Load UnitData asset from Resources or Addressables based on unitType
                // - Validate resource costs (credits, power, population cap)
                // - Add unit to production queue with timer
                // - Spawn unit at rally point when complete
                // Example: UnitData unitData = Resources.Load<UnitData>($"Units/{unitType}");
                //          if (unitData != null) producer.ProduceUnit(unitData);

                Debug.LogWarning($"Unit production not yet implemented: {unitType}");
            }
            else
            {
                Debug.LogWarning($"Building {building.GameObject.name} does not implement IUnitProducer");
            }
        }

        #endregion
    }
}
