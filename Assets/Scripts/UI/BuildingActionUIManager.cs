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

        [Header("UI Customization")]
        [Tooltip("Background image component to customize")]
        [SerializeField] private UnityEngine.UI.Image panelBackgroundImage;

        [Tooltip("Header/info section background")]
        [SerializeField] private UnityEngine.UI.Image headerBackgroundImage;

        [Tooltip("Optional accent border/highlight")]
        [SerializeField] private UnityEngine.UI.Image accentBorderImage;

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
            }

            if (buttonContainer == null)
            {
                buttonContainer = transform.Find("ButtonContainer");
            }

            // Ensure ButtonLayoutFixer is attached
            if (buttonContainer != null)
            {
                var layoutFixer = buttonContainer.GetComponent<ButtonLayoutFixer>();
                if (layoutFixer == null)
                {
                    layoutFixer = buttonContainer.gameObject.AddComponent<ButtonLayoutFixer>();
                }
            }

            if (infoDisplay == null)
            {
                infoDisplay = transform.Find("InfoSection")?.GetComponent<BuildingInfoDisplay>();
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
            else
            {
                Debug.LogError("BuildingActionUIManager: actionPanel is null in Start()!");
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

            // Apply UI customization from BuildingActionConfig (colors, backgrounds)
            ApplyUICustomization();

            // Update info display
            if (infoDisplay != null)
            {
                infoDisplay.UpdateDisplay(currentBuilding);
            }

            // Clear existing buttons
            ClearButtons();

            // Create new buttons for available actions
            CreateActionButtons();

            // Apply button layout settings AFTER buttons are created
            var actionConfig = currentBuildingActions?.GetActionConfig();
            if (actionConfig != null)
            {
                ApplyButtonLayoutSizing(actionConfig);
            }

            // Force immediate layout rebuild to prevent overlapping (per CLAUDE.md: handle edge cases)
            StartCoroutine(RebuildLayoutNextFrame());

            // Initial button state update (event-driven updates will handle subsequent changes)
            UpdateButtonStates();
        }

        private void ApplyUICustomization()
        {
            if (currentBuildingActions == null)
                return;

            var actionConfig = currentBuildingActions.GetActionConfig();
            if (actionConfig == null)
                return;

            // Apply panel background color
            if (panelBackgroundImage != null)
            {
                panelBackgroundImage.color = actionConfig.panelBackgroundColor;

                // Apply background sprite if provided
                if (actionConfig.panelBackgroundSprite != null)
                {
                    panelBackgroundImage.sprite = actionConfig.panelBackgroundSprite;
                }
            }

            // Apply header color
            if (headerBackgroundImage != null)
            {
                headerBackgroundImage.color = actionConfig.headerColor;
            }

            // Apply accent border color
            if (accentBorderImage != null)
            {
                accentBorderImage.color = actionConfig.accentColor;
            }

            // Apply button layout settings (layout component only, sizing done after buttons created)
            ApplyButtonLayoutType(actionConfig);
        }

        private void ApplyButtonLayoutType(RTS.Data.BuildingActionConfig actionConfig)
        {
            if (buttonContainer == null)
                return;

            // Remove ALL existing layout components immediately
            // Must use DestroyImmediate here because we're adding a new layout component right after
            // and Unity doesn't allow multiple LayoutGroup components on the same GameObject
            var existingLayouts = buttonContainer.GetComponents<UnityEngine.UI.LayoutGroup>();
            foreach (var layout in existingLayouts)
            {
                if (layout != null)
                {
                    DestroyImmediate(layout);
                }
            }

            // Also remove ContentSizeFitter immediately
            var existingFitter = buttonContainer.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (existingFitter != null)
            {
                DestroyImmediate(existingFitter);
            }

            // Apply padding
            var rectTransform = buttonContainer.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.offsetMin = new Vector2(actionConfig.buttonContainerPadding.x, actionConfig.buttonContainerPadding.w);
                rectTransform.offsetMax = new Vector2(-actionConfig.buttonContainerPadding.y, -actionConfig.buttonContainerPadding.z);
            }

            // Add appropriate layout component based on type
            switch (actionConfig.layoutType)
            {
                case RTS.Data.ButtonLayoutType.Grid:
                    var gridLayout = buttonContainer.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                    if (gridLayout != null)
                    {
                        gridLayout.cellSize = actionConfig.buttonSize;
                        gridLayout.spacing = new Vector2(actionConfig.buttonSpacing, actionConfig.buttonSpacing);
                        gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                        gridLayout.constraintCount = actionConfig.gridColumns;
                        gridLayout.childAlignment = TextAnchor.UpperLeft; // Start from top-left to prevent overlap
                        gridLayout.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
                        gridLayout.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
                        gridLayout.padding = new UnityEngine.RectOffset(0, 0, 0, 0); // No extra padding
                    }

                    // Add ContentSizeFitter to make container size itself to fit buttons
                    var gridFitter = buttonContainer.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    if (gridFitter != null)
                    {
                        gridFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                        gridFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    }
                    break;

                case RTS.Data.ButtonLayoutType.Horizontal:
                    var horizontalLayout = buttonContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                    if (horizontalLayout != null)
                    {
                        horizontalLayout.spacing = actionConfig.buttonSpacing;
                        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
                        horizontalLayout.childForceExpandWidth = false;
                        horizontalLayout.childForceExpandHeight = false;
                        horizontalLayout.childControlWidth = true;
                        horizontalLayout.childControlHeight = true;
                        horizontalLayout.padding = new UnityEngine.RectOffset(0, 0, 0, 0);
                    }

                    // Add ContentSizeFitter
                    var horizontalFitter = buttonContainer.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    if (horizontalFitter != null)
                    {
                        horizontalFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                        horizontalFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    }
                    break;

                case RTS.Data.ButtonLayoutType.Vertical:
                    var verticalLayout = buttonContainer.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                    if (verticalLayout != null)
                    {
                        verticalLayout.spacing = actionConfig.buttonSpacing;
                        verticalLayout.childAlignment = TextAnchor.MiddleCenter; // Center buttons
                        verticalLayout.childForceExpandWidth = false;
                        verticalLayout.childForceExpandHeight = false;
                        verticalLayout.childControlWidth = true;
                        verticalLayout.childControlHeight = true;
                        verticalLayout.padding = new UnityEngine.RectOffset(0, 0, 0, 0);
                    }

                    // Add ContentSizeFitter
                    var verticalFitter = buttonContainer.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    if (verticalFitter != null)
                    {
                        verticalFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                        verticalFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    }
                    break;
            }
        }

        private void ApplyButtonLayoutSizing(RTS.Data.BuildingActionConfig actionConfig)
        {
            if (buttonContainer == null || actionConfig == null)
                return;

            // Apply button sizing based on layout type
            foreach (Transform child in buttonContainer)
            {
                if (child == null) continue;

                // For Grid layout, remove ALL layout-related components to let GridLayoutGroup control everything
                if (actionConfig.layoutType == RTS.Data.ButtonLayoutType.Grid)
                {
                    // Remove LayoutElement if it exists (use DestroyImmediate for immediate cleanup)
                    var layoutElement = child.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (layoutElement != null)
                    {
                        DestroyImmediate(layoutElement);
                    }

                    // Remove ContentSizeFitter if it exists
                    var contentFitter = child.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                    if (contentFitter != null)
                    {
                        DestroyImmediate(contentFitter);
                    }

                    // Don't manually set size - let GridLayoutGroup handle it via cellSize
                }
                // For Horizontal and Vertical layouts, use LayoutElement to control button sizes
                else if (actionConfig.layoutType == RTS.Data.ButtonLayoutType.Horizontal ||
                         actionConfig.layoutType == RTS.Data.ButtonLayoutType.Vertical)
                {
                    var layoutElement = child.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (layoutElement == null)
                        layoutElement = child.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();

                    if (layoutElement != null)
                    {
                        layoutElement.preferredWidth = actionConfig.buttonSize.x;
                        layoutElement.preferredHeight = actionConfig.buttonSize.y;
                        layoutElement.minWidth = actionConfig.buttonSize.x;
                        layoutElement.minHeight = actionConfig.buttonSize.y;
                    }
                }
            }

            // Layout rebuild is handled by RebuildLayoutNextFrame() coroutine
        }

        /// <summary>
        /// Coroutine to rebuild layout on next frame to prevent button overlap.
        /// Unity UI layout groups sometimes need a frame to properly calculate positions.
        /// </summary>
        private System.Collections.IEnumerator RebuildLayoutNextFrame()
        {
            yield return null; // Wait one frame

            if (buttonContainer != null)
            {
                // Force canvas update
                Canvas.ForceUpdateCanvases();

                // Rebuild layout
                var containerRectTransform = buttonContainer.GetComponent<RectTransform>();
                if (containerRectTransform != null)
                {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
                }
            }
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
                return;
            }

            if (buttonContainer == null)
            {
                return;
            }

            if (actionButtonPrefab == null)
            {
                return;
            }

            var actionConfig = currentBuildingActions.GetActionConfig();
            if (actionConfig == null)
            {
                return;
            }

            if (actionConfig.actions == null || actionConfig.actions.Length == 0)
            {
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
                    Destroy(buttonObj);
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

        // Event-driven approach: Update button states when resources change
        // instead of polling in Update() - per CLAUDE.md performance guidelines
        private void OnEnable()
        {
            // Subscribe to resource change events if ResourceManager exists
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged += UpdateButtonStates;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= UpdateButtonStates;
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
        }
#endif
    }
}
