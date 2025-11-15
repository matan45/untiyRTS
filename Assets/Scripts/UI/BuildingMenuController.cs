using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildingMenuController : MonoBehaviour
{
    public static BuildingMenuController Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private Transform buildingButtonContainer;
    [SerializeField] private GameObject buildingButtonPrefab;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI powerText;
    
    [Header("Buildings")]
    public List<BuildingData> availableBuildings = new List<BuildingData>();
    
    private List<BuildingButton> buildingButtons = new List<BuildingButton>();
    private BuildingData selectedBuilding;
    
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
    
    void Start()
    {
        InitializeButtons();
        SubscribeToEvents();
        UpdateResourceDisplay();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeButtons()
    {
        // Clear existing buttons
        foreach (var button in buildingButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        buildingButtons.Clear();
        
        // Create buttons for each building
        foreach (var buildingData in availableBuildings)
        {
            if (buildingData == null || buildingButtonPrefab == null)
                continue;
                
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingButtonContainer);
            BuildingButton button = buttonObj.GetComponent<BuildingButton>();
            
            if (button != null)
            {
                button.Initialize(buildingData);
                buildingButtons.Add(button);
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnCreditsChanged += OnCreditsChanged;
            ResourceManager.Instance.OnPowerChanged += OnPowerChanged;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnCreditsChanged -= OnCreditsChanged;
            ResourceManager.Instance.OnPowerChanged -= OnPowerChanged;
        }
    }
    
    private void OnCreditsChanged(int newCredits)
    {
        UpdateResourceDisplay();
        UpdateButtonStates();
    }
    
    private void OnPowerChanged(int available, int total)
    {
        UpdateResourceDisplay();
        UpdateButtonStates();
    }
    
    private void UpdateResourceDisplay()
    {
        if (ResourceManager.Instance == null)
            return;
            
        if (creditsText != null)
            creditsText.text = $"${ResourceManager.Instance.Credits}";
            
        if (powerText != null)
            powerText.text = $"{ResourceManager.Instance.AvailablePower}/{ResourceManager.Instance.TotalPower}";
    }
    
    private void UpdateButtonStates()
    {
        foreach (var button in buildingButtons)
        {
            if (button != null)
            {
                button.UpdateLockState();
                button.UpdateAffordableState();
            }
        }
    }
    
    public void SelectBuilding(BuildingData building)
    {
        selectedBuilding = building;

        // Start placement mode
        if (BuildingPlacer.Instance != null)
        {
            BuildingPlacer.Instance.StartPlacement(building);
        }
    }
    
    public void DeselectBuilding()
    {
        selectedBuilding = null;
    }
    
    public BuildingData GetSelectedBuilding()
    {
        return selectedBuilding;
    }
}