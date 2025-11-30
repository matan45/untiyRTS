using UnityEngine;
using RTS.Data;

namespace RTS.Data
{
    [CreateAssetMenu(fileName = "New Building", menuName = "RTS/Building Data")]
    public class BuildingData : ScriptableObject
    {
    [Header("Building Info")]
    public string buildingName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    
    [Header("Costs")]
    public int creditsCost;
    public int powerRequired;
    public float buildTime = 5f;

    [Header("Turn-Based Construction")]
    [Tooltip("Number of turns to complete construction in turn-based mode.")]
    public int turnsToComplete = 3;
    
    [Header("Placement")]
    public Vector2Int size = new Vector2Int(2, 2); // Grid size
    public bool requiresPower = false;
    
    [Header("Prerequisites")]
    public BuildingData[] requiredBuildings;
    public int techLevel = 0;
    
    [Header("Production")]
    public bool canProduceUnits = false;
    public bool ProvidesPower = false;
    public int PowerProvided = 0;

    [Header("Actions")]
    [Tooltip("Configuration for building actions (sell, upgrade, unit production, etc.)")]
    public BuildingActionConfig actionConfig;
    
        public bool HasPrerequisites(Buildings.BuildingManager buildingManager)
        {
            if (requiredBuildings == null || requiredBuildings.Length == 0)
                return true;

            foreach (var required in requiredBuildings)
            {
                if (!buildingManager.HasBuilding(required))
                    return false;
            }

            return true;
        }
    }
}