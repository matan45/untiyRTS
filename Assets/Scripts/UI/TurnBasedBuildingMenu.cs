using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using RTS.Terrain;
using RTS.Data;
using RTS.Buildings;

namespace RTS.UI
{
    /// <summary>
    /// Turn-based building menu controller.
    /// Adapted from BuildingMenuController for hex grid placement.
    /// Displays available buildings, handles selection, triggers HexBuildingPlacer.
    /// NOTE: This is a SCENE-SPECIFIC singleton - UI is per-level.
    /// The instance will be destroyed when loading a new scene.
    /// </summary>
    public class TurnBasedBuildingMenu : MonoBehaviour
    {
        public static TurnBasedBuildingMenu Instance { get; private set; }

        [Header("Singleton Settings")]
        [SerializeField, Tooltip("If true, this UI will persist across scene loads. Typically FALSE for scene-specific UI.")]
        private bool persistAcrossScenes = false;

        [Header("UI References")]
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject buildingButtonPrefab;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI powerText;

        [Header("Building Data")]
        [SerializeField] private List<BuildingData> availableBuildings = new List<BuildingData>();

        [Header("Settings")]
        [SerializeField] private bool autoLoadBuildings = true;

        private List<BuildingButton> buildingButtons = new List<BuildingButton>();
        private BuildingData selectedBuilding;
        private ResourceManager resourceManager;
        private BuildingManager buildingManager;
        private UIMessageDisplay messageDisplay;

        private void Awake()
        {
            // Singleton pattern with optional persistence
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"TurnBasedBuildingMenu: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Optional persistence (typically false for scene-specific UI)
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("TurnBasedBuildingMenu: Set to persist across scenes.");
            }
        }

        private void Start()
        {
            // Cache manager references
            resourceManager = FindFirstObjectByType<ResourceManager>();
            buildingManager = FindFirstObjectByType<BuildingManager>();
            messageDisplay = FindFirstObjectByType<UIMessageDisplay>();

            // Validate critical dependencies
            if (resourceManager == null)
            {
                Debug.LogWarning("TurnBasedBuildingMenu: ResourceManager not found in scene. Resource validation will be skipped.");
            }
            else
            {
                resourceManager.OnCreditsChanged += OnResourcesChanged;
                resourceManager.OnPowerChanged += OnPowerChangedHandler;
            }

            if (buildingManager == null)
            {
                Debug.LogWarning("TurnBasedBuildingMenu: BuildingManager not found in scene. Prerequisite checks will be skipped.");
            }

            if (messageDisplay == null)
            {
                Debug.LogWarning("TurnBasedBuildingMenu: UIMessageDisplay not found in scene. User messages will only appear in console.");
            }

            // Auto-load buildings if enabled
            if (autoLoadBuildings && availableBuildings.Count == 0)
            {
                LoadAllBuildingData();
            }

            // Initialize UI
            InitializeButtons();
            UpdateResourceDisplay();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (resourceManager != null)
            {
                resourceManager.OnCreditsChanged -= OnResourcesChanged;
                resourceManager.OnPowerChanged -= OnPowerChangedHandler;
            }

            // Clear static instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Loads all BuildingData assets from Resources or specified folder.
        /// </summary>
        private void LoadAllBuildingData()
        {
            BuildingData[] buildings = Resources.LoadAll<BuildingData>("BuildingData");
            if (buildings.Length > 0)
            {
                availableBuildings.AddRange(buildings);
                Debug.Log($"TurnBasedBuildingMenu: Loaded {buildings.Length} buildings from Resources");
            }
            else
            {
                Debug.LogWarning("TurnBasedBuildingMenu: No buildings found in Resources/BuildingData");
            }
        }

        /// <summary>
        /// Creates UI buttons for all available buildings.
        /// </summary>
        private void InitializeButtons()
        {
            if (buttonContainer == null)
            {
                Debug.LogError("TurnBasedBuildingMenu: Button container not assigned!");
                return;
            }

            if (buildingButtonPrefab == null)
            {
                Debug.LogError("TurnBasedBuildingMenu: Building button prefab not assigned!");
                return;
            }

            // Clear existing buttons
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
            buildingButtons.Clear();

            // Create button for each building
            foreach (BuildingData building in availableBuildings)
            {
                GameObject buttonObj = Instantiate(buildingButtonPrefab, buttonContainer);
                BuildingButton button = buttonObj.GetComponent<BuildingButton>();

                if (button != null)
                {
                    button.Initialize(building);
                    buildingButtons.Add(button);
                }
            }
        }

        /// <summary>
        /// Called when a building button is clicked.
        /// </summary>
        public void SelectBuilding(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                Debug.LogWarning("TurnBasedBuildingMenu: Null building data");
                return;
            }

            // Check if we can afford it
            if (resourceManager != null)
            {
                if (!resourceManager.CanAfford(buildingData.creditsCost, buildingData.powerRequired))
                {
                    Debug.LogWarning($"Cannot afford {buildingData.buildingName}");
                    ShowMessage($"Insufficient resources for {buildingData.buildingName}");
                    return;
                }
            }

            // Check prerequisites
            if (buildingManager != null && buildingData.requiredBuildings != null)
            {
                foreach (BuildingData required in buildingData.requiredBuildings)
                {
                    if (!buildingManager.HasBuilding(required))
                    {
                        Debug.LogWarning($"Missing prerequisite: {required.buildingName}");
                        ShowMessage($"Requires: {required.buildingName}");
                        return;
                    }
                }
            }

            selectedBuilding = buildingData;

            // Start placement with HexBuildingPlacer
            if (HexBuildingPlacer.Instance != null)
            {
                HexBuildingPlacer.Instance.StartPlacement(buildingData);
            }
            else
            {
                Debug.LogError("TurnBasedBuildingMenu: HexBuildingPlacer not found!");
            }

            Debug.Log($"Selected building: {buildingData.buildingName}");
        }

        /// <summary>
        /// Deselects current building and cancels placement.
        /// </summary>
        public void DeselectBuilding()
        {
            selectedBuilding = null;

            if (HexBuildingPlacer.Instance != null)
            {
                HexBuildingPlacer.Instance.CancelPlacement();
            }
        }

        /// <summary>
        /// Updates resource display text.
        /// </summary>
        private void UpdateResourceDisplay()
        {
            if (resourceManager == null) return;

            if (creditsText != null)
            {
                creditsText.text = $"Credits: {resourceManager.Credits}";
            }

            if (powerText != null)
            {
                powerText.text = $"Power: {resourceManager.AvailablePower}/{resourceManager.TotalPower}";
            }

            // Update button states based on affordability
            UpdateButtonStates();
        }

        /// <summary>
        /// Updates all building buttons based on resources and prerequisites.
        /// </summary>
        private void UpdateButtonStates()
        {
            foreach (BuildingButton button in buildingButtons)
            {
                button.UpdateUI();
            }
        }

        /// <summary>
        /// Shows a temporary message to the user.
        /// </summary>
        private void ShowMessage(string message)
        {
            // Use cached UIMessageDisplay reference
            if (messageDisplay != null)
            {
                messageDisplay.ShowMessage(message, UnityEngine.Color.white);
            }
            else
            {
                Debug.Log($"MESSAGE: {message}");
            }
        }

        /// <summary>
        /// Public method to add a building to the available list.
        /// </summary>
        public void AddBuilding(BuildingData buildingData)
        {
            if (!availableBuildings.Contains(buildingData))
            {
                availableBuildings.Add(buildingData);
                InitializeButtons(); // Refresh UI
            }
        }

        /// <summary>
        /// Public method to remove a building from the available list.
        /// </summary>
        public void RemoveBuilding(BuildingData buildingData)
        {
            if (availableBuildings.Contains(buildingData))
            {
                availableBuildings.Remove(buildingData);
                InitializeButtons(); // Refresh UI
            }
        }

        // Event handlers (named methods to allow proper unsubscription)
        private void OnResourcesChanged(int credits) => UpdateResourceDisplay();
        private void OnPowerChangedHandler(int available, int total) => UpdateResourceDisplay();
    }
}
