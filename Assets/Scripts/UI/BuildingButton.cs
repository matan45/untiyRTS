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
        // Auto-wire references if not set
        if (button == null)
            button = GetComponent<Button>();

        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();

        if (nameText == null)
            nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

        if (costText == null)
            costText = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();

        if (lockOverlay == null)
            lockOverlay = transform.Find("LockOverlay")?.GetComponent<Image>();

        if (button != null)
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
        if (buildingData == null)
        {
            Debug.LogWarning("BuildingButton: Cannot update lock state - buildingData is null");
            return;
        }

        if (BuildingManager.Instance == null)
        {
            // Assume unlocked if no BuildingManager
            isUnlocked = true;
            Debug.LogWarning($"BuildingButton ({buildingData.buildingName}): BuildingManager.Instance is null. Assuming unlocked.");
        }
        else
        {
            isUnlocked = buildingData.HasPrerequisites(BuildingManager.Instance);
        }

        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!isUnlocked);

        if (button != null)
            button.interactable = isUnlocked;
    }
    
    public void UpdateAffordableState()
    {
        if (buildingData == null)
        {
            Debug.LogWarning("BuildingButton: Cannot update affordable state - buildingData is null");
            return;
        }

        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning($"BuildingButton ({buildingData.buildingName}): ResourceManager.Instance is null. Assuming affordable.");
            return;
        }

        bool canAfford = ResourceManager.Instance.CanAfford(
            buildingData.creditsCost,
            buildingData.powerRequired
        );

        // Only disable if also unlocked
        if (isUnlocked && button != null)
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