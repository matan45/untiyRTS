using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTS.Data;
using RTS.Buildings;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image lockOverlay;
    [SerializeField] private Button button;
    
    private BuildingData buildingData;
    private bool isUnlocked = false;
    
    public BuildingData Data => buildingData;
    
    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        button.onClick.AddListener(OnButtonClicked);
    }
    
    public void Initialize(BuildingData data)
    {
        buildingData = data;
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (buildingData == null)
            return;
            
        // Update icon
        if (iconImage != null)
        {
            iconImage.sprite = buildingData.icon;
            iconImage.enabled = buildingData.icon != null;
        }
        
        // Update name
        if (nameText != null)
            nameText.text = buildingData.buildingName;
            
        // Update cost
        if (costText != null)
            costText.text = $"${buildingData.creditsCost}";
            
        // Check if unlocked
        UpdateLockState();
        
        // Check if affordable
        UpdateAffordableState();
    }
    
    public void UpdateLockState()
    {
        if (BuildingManager.Instance == null || buildingData == null)
            return;
            
        isUnlocked = buildingData.HasPrerequisites(BuildingManager.Instance);
        
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!isUnlocked);
            
        button.interactable = isUnlocked;
    }
    
    public void UpdateAffordableState()
    {
        if (ResourceManager.Instance == null || buildingData == null)
            return;
            
        bool canAfford = ResourceManager.Instance.CanAfford(
            buildingData.creditsCost,
            buildingData.powerRequired
        );
        
        // Only disable if also unlocked
        if (isUnlocked)
        {
            button.interactable = canAfford;
            
            // Visual feedback for affordability
            if (costText != null)
            {
                costText.color = canAfford ? Color.white : Color.red;
            }
        }
    }
    
    private void OnButtonClicked()
    {
        if (buildingData != null && BuildingMenuController.Instance != null)
        {
            BuildingMenuController.Instance.SelectBuilding(buildingData);
        }
    }
}