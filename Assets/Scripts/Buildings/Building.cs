using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;
    
    public BuildingData Data => buildingData;
    public bool IsConstructed { get; private set; }
    
    private float constructionProgress = 0f;
    
    void Start()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.RegisterBuilding(this);
        }
    }
    
    void OnDestroy()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterBuilding(this);
        }
    }
    
    public void StartConstruction()
    {
        IsConstructed = false;
        constructionProgress = 0f;
    }
    
    public void UpdateConstruction(float deltaTime)
    {
        if (IsConstructed)
            return;
            
        if (buildingData != null && buildingData.buildTime > 0)
        {
            constructionProgress += deltaTime;
            
            if (constructionProgress >= buildingData.buildTime)
            {
                CompleteConstruction();
            }
        }
    }
    
    public float GetConstructionProgress()
    {
        if (buildingData == null || buildingData.buildTime <= 0)
            return 1f;
            
        return Mathf.Clamp01(constructionProgress / buildingData.buildTime);
    }
    
    private void CompleteConstruction()
    {
        IsConstructed = true;
        constructionProgress = buildingData.buildTime;
        
        // Add power if this building provides it
        if (buildingData.providespower && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddPower(buildingData.powerProvided);
        }
    }
    
    public void SetBuildingData(BuildingData data)
    {
        buildingData = data;
    }
}