using UnityEngine;
using System.Collections.Generic;
using RTS.Data;

namespace RTS.Buildings
{
    /// <summary>
    /// Manages placed buildings and tracks building counts.
    /// NOTE: This is a SCENE-SPECIFIC singleton - building state resets per level.
    /// The instance will be destroyed when loading a new scene.
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }

        [Header("Singleton Settings")]
        [SerializeField, Tooltip("If true, building state will persist across scene loads. Typically FALSE for level-based games.")]
        private bool persistAcrossScenes = false;

        private List<Building> placedBuildings = new List<Building>();
        private Dictionary<BuildingData, int> buildingCounts = new Dictionary<BuildingData, int>();

        void Awake()
        {
            // Singleton pattern with optional persistence
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"BuildingManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Optional persistence (typically false for scene-specific buildings)
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("BuildingManager: Set to persist across scenes.");
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

        void OnDestroy()
        {
            // Clear static instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}