using System.Collections.Generic;
using UnityEngine;
using RTS.Interfaces;
using RTS.Selection;
using RTS.Buildings;

namespace RTS.UI
{
    /// <summary>
    /// Manages the building action UI panel that appears at the bottom-center when a building is selected.
    /// Implements Observer Pattern - subscribes to selection events.
    /// Implements Single Responsibility Principle (SRP) - only manages building action UI.
    /// </summary>
    public class BuildingActionUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Root panel that contains all building action UI (show/hide this)")]
        [SerializeField] private GameObject actionPanel;

        [Tooltip("Container where action buttons will be spawned")]
        [SerializeField] private Transform buttonContainer;

        [Tooltip("Prefab for individual action buttons")]
        [SerializeField] private GameObject actionButtonPrefab;

        [Header("Info Display")]
        [Tooltip("Component that displays building information")]
        [SerializeField] private BuildingInfoDisplay infoDisplay;

        // Currently selected building
        private Building currentBuilding;
        private IBuildingActions currentBuildingActions;

        // Active button instances
        private List<BuildingActionButton> activeButtons = new List<BuildingActionButton>();

        private void Awake()
        {
            // Auto-wire references if not set
            if (actionPanel == null)
            {
                actionPanel = gameObject;
                Debug.Log("BuildingActionUIManager: Auto-assigned actionPanel to self");
            }

            if (buttonContainer == null)
            {
                buttonContainer = transform.Find("ButtonContainer");
                Debug.Log("BuildingActionUIManager: Auto-found ButtonContainer");
            }

            // Ensure ButtonLayoutFixer is attached
            if (buttonContainer != null)
            {
                var layoutFixer = buttonContainer.GetComponent<ButtonLayoutFixer>();
                if (layoutFixer == null)
                {
                    layoutFixer = buttonContainer.gameObject.AddComponent<ButtonLayoutFixer>();
                    Debug.Log("BuildingActionUIManager: Added ButtonLayoutFixer to ButtonContainer");
                }
            }

            if (infoDisplay == null)
            {
                infoDisplay = transform.Find("InfoSection")?.GetComponent<BuildingInfoDisplay>();
                Debug.Log("BuildingActionUIManager: Auto-found BuildingInfoDisplay");
            }
        }

        private void Start()
        {
            // Subscribe to selection events
            if (BuildingSelectionManager.Instance != null)
            {
                BuildingSelectionManager.Instance.OnSelectionChanged += OnBuildingSelected;
                BuildingSelectionManager.Instance.OnSelectionCleared += OnSelectionCleared;
            }
            else
            {
                Debug.LogWarning("BuildingActionUIManager: BuildingSelectionManager.Instance is null!");
            }

            // Hide panel initially
            if (actionPanel != null)
            {
                actionPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (BuildingSelectionManager.Instance != null)
            {
                BuildingSelectionManager.Instance.OnSelectionChanged -= OnBuildingSelected;
                BuildingSelectionManager.Instance.OnSelectionCleared -= OnSelectionCleared;
            }
        }

        private void OnBuildingSelected(ISelectable selectable)
        {
            // Check if the selected object is a building with actions
            if (selectable is Building building && building is IBuildingActions buildingActions)
            {
                // Only show action panel if building is fully constructed
                if (building.IsConstructed)
                {
                    currentBuilding = building;
                    currentBuildingActions = buildingActions;

                    ShowActionPanel();
                }
                else
                {
                    // Building is still under construction - don't show action panel
                    HideActionPanel();
                }
            }
            else
            {
                // Selected something that's not a building or doesn't have actions
                HideActionPanel();
            }
        }

        private void OnSelectionCleared()
        {
            HideActionPanel();
        }

        private void ShowActionPanel()
        {
            if (actionPanel == null)
            {
                Debug.LogError("BuildingActionUIManager: actionPanel is not assigned!");
                return;
            }

            actionPanel.SetActive(true);

            // Update info display
            if (infoDisplay != null)
            {
                infoDisplay.UpdateDisplay(currentBuilding);
            }

            // Clear existing buttons
            ClearButtons();

            // Create new buttons for available actions
            CreateActionButtons();
        }

        private void HideActionPanel()
        {
            if (actionPanel != null)
            {
                actionPanel.SetActive(false);
            }

            currentBuilding = null;
            currentBuildingActions = null;

            ClearButtons();
        }

        private void CreateActionButtons()
        {
            if (currentBuildingActions == null)
            {
                Debug.LogError("BuildingActionUIManager: currentBuildingActions is null!");
                return;
            }

            if (buttonContainer == null)
            {
                Debug.LogError("BuildingActionUIManager: buttonContainer is null!");
                return;
            }

            if (actionButtonPrefab == null)
            {
                Debug.LogError("BuildingActionUIManager: actionButtonPrefab is not assigned!");
                return;
            }

            var actionConfig = currentBuildingActions.GetActionConfig();
            if (actionConfig == null)
            {
                Debug.LogWarning($"Building {currentBuilding.GameObject.name} has null actionConfig!");
                return;
            }

            if (actionConfig.actions == null || actionConfig.actions.Length == 0)
            {
                Debug.LogWarning($"Building {currentBuilding.GameObject.name} has no actions configured");
                return;
            }

            // Create a button for each action
            foreach (var actionData in actionConfig.actions)
            {
                if (actionData == null) continue;

                GameObject buttonObj = Instantiate(actionButtonPrefab, buttonContainer);
                var actionButton = buttonObj.GetComponent<BuildingActionButton>();

                if (actionButton != null)
                {
                    actionButton.Initialize(actionData, currentBuildingActions);
                    activeButtons.Add(actionButton);
                }
                else
                {
                    Debug.LogWarning("Action button prefab does not have BuildingActionButton component!");
                    Destroy(buttonObj);
                }
            }

            Debug.Log($"Created {activeButtons.Count} action buttons for {currentBuilding.GameObject.name}");

            // Force layout rebuild after creating buttons
            if (buttonContainer != null)
            {
                // Fix button sizes by adding LayoutElements
                var layoutFixer = buttonContainer.GetComponent<ButtonLayoutFixer>();
                if (layoutFixer != null)
                {
                    layoutFixer.FixButtonSizes();
                }

                var containerRect = buttonContainer.GetComponent<RectTransform>();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
                Debug.Log("Forced layout rebuild on ButtonContainer");

                // Log button positions for debugging
                Debug.Log($"ButtonContainer size: {containerRect.rect.size}, position: {containerRect.anchoredPosition}");
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    var child = buttonContainer.GetChild(i);
                    var childRect = child.GetComponent<RectTransform>();
                    var buttonComponent = child.GetComponent<BuildingActionButton>();
                    string actionName = buttonComponent != null ? buttonComponent.name : "Unknown";
                    Debug.Log($"  Button {i} [{actionName}]: position={childRect.anchoredPosition}, size={childRect.rect.size}, anchors=({childRect.anchorMin}, {childRect.anchorMax})");
                }
            }
        }

        private void ClearButtons()
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            activeButtons.Clear();
        }

        private void Update()
        {
            // Update button states periodically (every 10 frames to reduce overhead)
            if (currentBuilding != null && Time.frameCount % 10 == 0)
            {
                UpdateButtonStates();
            }
        }

        private void UpdateButtonStates()
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                {
                    button.UpdateState();
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Find UI References")]
        private void FindUIReferences()
        {
            if (actionPanel == null)
            {
                actionPanel = transform.Find("ActionPanel")?.gameObject;
            }

            if (buttonContainer == null && actionPanel != null)
            {
                buttonContainer = actionPanel.transform.Find("ButtonContainer");
            }

            if (infoDisplay == null && actionPanel != null)
            {
                infoDisplay = actionPanel.GetComponentInChildren<BuildingInfoDisplay>();
            }

            Debug.Log("UI References search complete");
        }
#endif
    }
}
