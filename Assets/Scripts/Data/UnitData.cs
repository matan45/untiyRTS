using UnityEngine;

namespace RTS.Data
{
    /// <summary>
    /// ScriptableObject defining a unit type that can be produced by buildings.
    /// Similar to BuildingData, this is a data-driven approach for unit configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitData", menuName = "RTS/Unit Data", order = 3)]
    public class UnitData : ScriptableObject
    {
        [Header("Unit Identity")]
        [Tooltip("Display name of the unit")]
        public string unitName;

        [Header("Prefab")]
        [Tooltip("Unit prefab to spawn when production completes")]
        public GameObject prefab;

        [Header("Production")]
        [Tooltip("Credit cost to produce this unit")]
        public int creditsCost;

        [Tooltip("Power required to produce this unit")]
        public int powerCost;

        [Tooltip("Time in seconds to produce this unit")]
        public float productionTime = 5f;

        [Header("Prerequisites")]
        [Tooltip("Buildings required before this unit can be produced")]
        public BuildingData[] requiredBuildings;

      
        public bool HasPrerequisites(Buildings.BuildingManager buildingManager)
        {
            if (requiredBuildings == null || requiredBuildings.Length == 0)
                return true;

            foreach (var requiredBuilding in requiredBuildings)
            {
                if (requiredBuilding != null && !buildingManager.HasBuilding(requiredBuilding))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(unitName))
            {
                unitName = name;
            }

            if (productionTime < 0.1f)
            {
                productionTime = 0.1f;
            }
        }
    }
}
