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
    /// </summary>
    public class UnitsMenu : MonoBehaviour
    {
        public static UnitsMenu Instance { get; private set; }

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Find ResourceManager
            resourceManager = FindFirstObjectByType<ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.OnCreditsChanged += (credits) => UpdateButtonStates();
                resourceManager.OnPowerChanged += (available, total) => UpdateButtonStates();
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
            if (resourceManager != null)
            {
                resourceManager.OnCreditsChanged -= (credits) => UpdateButtonStates();
                resourceManager.OnPowerChanged -= (available, total) => UpdateButtonStates();
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
                    button.Initialize(unit);
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
            BuildingManager buildingManager = FindFirstObjectByType<BuildingManager>();
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
            UIMessageDisplay messageDisplay = FindFirstObjectByType<UIMessageDisplay>();
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
