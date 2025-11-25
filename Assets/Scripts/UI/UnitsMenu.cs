using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using RTS.Data;
using RTS.Buildings;

namespace RTS.UI
{
    /// <summary>
    /// Units menu controller for displaying and selecting available units to produce.
    /// Similar to TurnBasedBuildingMenu but for unit production.
    /// NOTE: This is a SCENE-SPECIFIC singleton - UI is per-level.
    /// The instance will be destroyed when loading a new scene.
    /// </summary>
    public class UnitsMenu : MonoBehaviour
    {
        public static UnitsMenu Instance { get; private set; }

        [Header("Singleton Settings")]
        [SerializeField, Tooltip("If true, this UI will persist across scene loads. Typically FALSE for scene-specific UI.")]
        private bool persistAcrossScenes = false;

        [Header("UI References")]
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject unitButtonPrefab;

        [Header("Unit Data")]
        [SerializeField] private List<UnitData> availableUnits = new List<UnitData>();

        [Header("Settings")]
        [SerializeField] private bool autoLoadUnits = true;

        private List<UnitButton> unitButtons = new List<UnitButton>();
        private UnitData selectedUnit;
        private ResourceManager resourceManager;
        private BuildingManager buildingManager;
        private UIMessageDisplay messageDisplay;

        private void Awake()
        {
            // Singleton pattern with optional persistence
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"UnitsMenu: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Optional persistence (typically false for scene-specific UI)
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("UnitsMenu: Set to persist across scenes.");
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
                Debug.LogWarning("UnitsMenu: ResourceManager not found in scene. Resource validation will be skipped.");
            }
            else
            {
                resourceManager.OnCreditsChanged += (credits) => UpdateButtonStates();
                resourceManager.OnPowerChanged += (available, total) => UpdateButtonStates();
            }

            if (buildingManager == null)
            {
                Debug.LogWarning("UnitsMenu: BuildingManager not found in scene. Prerequisite checks will be skipped.");
            }

            if (messageDisplay == null)
            {
                Debug.LogWarning("UnitsMenu: UIMessageDisplay not found in scene. User messages will only appear in console.");
            }

            // Auto-load units if enabled
            if (autoLoadUnits && availableUnits.Count == 0)
            {
                LoadAllUnitData();
            }

            // Initialize UI
            InitializeButtons();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (resourceManager != null)
            {
                resourceManager.OnCreditsChanged -= (credits) => UpdateButtonStates();
                resourceManager.OnPowerChanged -= (available, total) => UpdateButtonStates();
            }

            // Clear static instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Loads all UnitData assets from Resources.
        /// </summary>
        private void LoadAllUnitData()
        {
            UnitData[] units = Resources.LoadAll<UnitData>("UnitData");
            if (units.Length > 0)
            {
                availableUnits.AddRange(units);
                Debug.Log($"UnitsMenu: Loaded {units.Length} units from Resources");
            }
            else
            {
                Debug.LogWarning("UnitsMenu: No units found in Resources/UnitData");
            }
        }

        /// <summary>
        /// Creates UI buttons for all available units.
        /// </summary>
        private void InitializeButtons()
        {
            if (buttonContainer == null)
            {
                Debug.LogError("UnitsMenu: Button container not assigned!");
                return;
            }

            if (unitButtonPrefab == null)
            {
                Debug.LogError("UnitsMenu: Unit button prefab not assigned!");
                return;
            }

            // Clear existing buttons
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
            unitButtons.Clear();

            // Create button for each unit
            foreach (UnitData unit in availableUnits)
            {
                GameObject buttonObj = Instantiate(unitButtonPrefab, buttonContainer);
                UnitButton button = buttonObj.GetComponent<UnitButton>();

                if (button != null)
                {
                    // Pass cached manager references to avoid repeated FindFirstObjectByType calls
                    button.Initialize(unit, resourceManager, buildingManager);
                    unitButtons.Add(button);
                }
            }
        }

        /// <summary>
        /// Called when a unit button is clicked.
        /// </summary>
        public void SelectUnit(UnitData unitData)
        {
            if (unitData == null)
            {
                Debug.LogWarning("UnitsMenu: Null unit data");
                return;
            }

            // Check if we can afford it
            if (resourceManager != null)
            {
                if (!resourceManager.CanAfford(unitData.creditsCost, unitData.powerCost))
                {
                    Debug.LogWarning($"Cannot afford {unitData.unitName}");
                    ShowMessage($"Insufficient resources for {unitData.unitName}");
                    return;
                }
            }

            // Check prerequisites
            if (buildingManager != null && !unitData.HasPrerequisites(buildingManager))
            {
                Debug.LogWarning($"Missing prerequisites for {unitData.unitName}");
                ShowMessage($"Missing required buildings for {unitData.unitName}");
                return;
            }

            selectedUnit = unitData;

            // TODO: Trigger unit production system
            // For now, just log the selection
            Debug.Log($"Selected unit for production: {unitData.unitName}");

            // Placeholder: Deduct resources and spawn unit immediately
            // In a real RTS, you'd queue this unit in a production building
            if (resourceManager != null)
            {
                if (resourceManager.SpendResources(unitData.creditsCost, unitData.powerCost))
                {
                    Debug.Log($"Unit {unitData.unitName} production started!");
                }
                else
                {
                    Debug.LogWarning($"Failed to spend resources for {unitData.unitName}");
                }
            }
        }

        /// <summary>
        /// Deselects current unit.
        /// </summary>
        public void DeselectUnit()
        {
            selectedUnit = null;
        }

        /// <summary>
        /// Updates all unit buttons based on resources and prerequisites.
        /// </summary>
        private void UpdateButtonStates()
        {
            foreach (UnitButton button in unitButtons)
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
                messageDisplay.ShowMessage(message, Color.white);
            }
            else
            {
                Debug.Log($"MESSAGE: {message}");
            }
        }

        /// <summary>
        /// Public method to add a unit to the available list.
        /// </summary>
        public void AddUnit(UnitData unitData)
        {
            if (!availableUnits.Contains(unitData))
            {
                availableUnits.Add(unitData);
                InitializeButtons();
            }
        }

        /// <summary>
        /// Public method to remove a unit from the available list.
        /// </summary>
        public void RemoveUnit(UnitData unitData)
        {
            if (availableUnits.Contains(unitData))
            {
                availableUnits.Remove(unitData);
                InitializeButtons();
            }
        }
    }
}
