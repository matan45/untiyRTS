using UnityEngine;
using System.Collections.Generic;
using RTS.Data;

namespace RTS.Buildings
{
    public class BuildingManager : MonoBehaviour
    {
    public static BuildingManager Instance { get; private set; }
    
    private List<Building> placedBuildings = new List<Building>();
    private Dictionary<BuildingData, int> buildingCounts = new Dictionary<BuildingData, int>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void RegisterBuilding(Building building)
    {
        if (!placedBuildings.Contains(building))
        {
            placedBuildings.Add(building);
            
            if (building.Data != null)
            {
                if (!buildingCounts.ContainsKey(building.Data))
                    buildingCounts[building.Data] = 0;
                    
                buildingCounts[building.Data]++;
            }
        }
    }
    
    public void UnregisterBuilding(Building building)
    {
        if (placedBuildings.Contains(building))
        {
            placedBuildings.Remove(building);
            
            if (building.Data != null && buildingCounts.ContainsKey(building.Data))
            {
                buildingCounts[building.Data]--;
                if (buildingCounts[building.Data] <= 0)
                    buildingCounts.Remove(building.Data);
            }
        }
    }
    
    public bool HasBuilding(BuildingData data)
    {
        return buildingCounts.ContainsKey(data) && buildingCounts[data] > 0;
    }
    
    public int GetBuildingCount(BuildingData data)
    {
        return buildingCounts.ContainsKey(data) ? buildingCounts[data] : 0;
    }
    
        public List<Building> GetAllBuildings()
        {
            return new List<Building>(placedBuildings);
        }
    }
}