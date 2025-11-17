using RTS.Actions;
using RTS.Data;
using RTS.Interfaces;
using RTS.Selection;
using TMPro;
using UnityEngine;

namespace RTS.Buildings
{
    public class Building : MonoBehaviour, ISelectable, IBuildingActions, IUpgradeable
    {
        [Header("Building Configuration")] [SerializeField]
        private BuildingData buildingData;

        [SerializeField] private BuildingActionConfig actionConfig;

        [Header("Upgrade Configuration")] [SerializeField]
        private BuildingData upgradeData;

        [Header("Selection Visual")] [SerializeField]
        private SelectionVisualController selectionVisual;

        [Header("Construction Display")]
        [Tooltip("3D Text to display construction progress (optional, will be auto-created if not assigned)")]
        [SerializeField]
        private TextMeshPro constructionText;

        [SerializeField] private float textHeightOffset = 0.2f;

        [Tooltip("Font size for construction progress text")]
        [SerializeField] private float constructionTextFontSize = 3f;

        [Tooltip("Outline width for construction text visibility")]
        [SerializeField] private float constructionTextOutlineWidth = 0.2f;

        public BuildingData Data => buildingData;
        public bool IsConstructed { get; private set; }

        // ISelectable implementation
        public bool IsSelected { get; private set; }
        public GameObject GameObject => gameObject;

        private float constructionProgress = 0f;
        private bool isUpgrading = false;
        private float upgradeProgress = 0f;
        private GameObject constructionTextObject;

        // Cached references for performance (per CLAUDE.md guidelines)
        private Camera mainCamera;
        private Renderer buildingRenderer;
        private float cachedBuildingHeight;

        void Start()
        {
            // Initialize actionConfig from buildingData if not set
            if (buildingData != null && actionConfig == null)
            {
                actionConfig = buildingData.actionConfig;
            }

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.RegisterBuilding(this);
            }

            // Auto-find selection visual if not assigned
            if (selectionVisual == null)
            {
                selectionVisual = GetComponent<SelectionVisualController>();
            }

            // Create construction text if not assigned
            if (constructionText == null)
            {
                CreateConstructionText();
            }

            // Hide construction text initially if building is already constructed
            if (IsConstructed && constructionText != null)
            {
                constructionText.gameObject.SetActive(false);
            }

            // Cache references for performance (per CLAUDE.md guidelines - avoid lookups in Update)
            mainCamera = Camera.main;
            buildingRenderer = GetComponentInChildren<Renderer>();
            if (buildingRenderer != null)
            {
                cachedBuildingHeight = buildingRenderer.bounds.size.y;
            }
        }

        void OnDestroy()
        {
            // Deselect if currently selected
            if (IsSelected && BuildingSelectionManager.Instance != null)
            {
                BuildingSelectionManager.Instance.ClearSelection();
            }

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterBuilding(this);
            }

            // Clean up construction text if it was auto-created
            if (constructionTextObject != null)
            {
                Destroy(constructionTextObject);
            }
        }

        void Update()
        {
            // Update construction text to face camera and position
            // Uses cached references for performance (per CLAUDE.md guidelines)
            if (constructionText != null && constructionText.gameObject.activeSelf && mainCamera != null)
            {
                // Position text at top of building + offset
                constructionText.transform.position =
                    transform.position + Vector3.up * (cachedBuildingHeight + textHeightOffset);

                // Face the camera
                constructionText.transform.LookAt(mainCamera.transform);
                constructionText.transform.Rotate(0, 180, 0); // Face the camera properly
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Update text position in editor when textHeightOffset changes
            if (constructionText != null && Application.isPlaying)
            {
                // Position will be updated in Update() method
            }
        }
#endif

        private void CreateConstructionText()
        {
            // Create a new GameObject for the 3D text
            constructionTextObject = new GameObject("ConstructionText");
            constructionTextObject.transform.SetParent(transform);
            constructionTextObject.transform.localPosition = Vector3.up * textHeightOffset;

            // Add TextMeshPro component
            constructionText = constructionTextObject.AddComponent<TextMeshPro>();

            // Configure text appearance (per CLAUDE.md: avoid magic numbers)
            constructionText.fontSize = constructionTextFontSize;
            constructionText.alignment = TextAlignmentOptions.Center;
            constructionText.color = Color.yellow;
            constructionText.text = "0%";

            // Add outline for better visibility
            constructionText.outlineWidth = constructionTextOutlineWidth;
            constructionText.outlineColor = Color.black;

            // Set rendering settings
            constructionText.GetComponent<MeshRenderer>().sortingOrder = 100; // Render on top
        }

        private void UpdateConstructionText()
        {
            if (constructionText == null)
                return;

            if (!IsConstructed)
            {
                // Show and update text during construction
                if (!constructionText.gameObject.activeSelf)
                    constructionText.gameObject.SetActive(true);

                // Calculate percentage based on build time
                float progress = GetConstructionProgress(); // Returns 0-1
                int percentage = Mathf.RoundToInt(progress * 100f);
                constructionText.text = $"{percentage}%";
            }
            else
            {
                // Hide text when construction is complete
                if (constructionText.gameObject.activeSelf)
                    constructionText.gameObject.SetActive(false);
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

                // Update the construction percentage text
                UpdateConstructionText();

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

            // Hide construction text
            UpdateConstructionText();

            // Add power if this building provides it
            if (buildingData.providespower && ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddPower(buildingData.powerProvided);
            }
        }

        public void SetBuildingData(BuildingData data)
        {
            buildingData = data;
            if (buildingData != null)
            {
                actionConfig = buildingData.actionConfig;
            }
        }

        #region ISelectable Implementation

        public void Select()
        {
            if (IsSelected) return;

            IsSelected = true;

            // Show selection visual
            if (selectionVisual != null)
            {
                selectionVisual.ShowSelection(actionConfig);
            }
        }

        public void Deselect()
        {
            if (!IsSelected) return;

            IsSelected = false;

            // Hide selection visual
            if (selectionVisual != null)
            {
                selectionVisual.HideSelection();
            }
        }

        #endregion

        #region IBuildingActions Implementation

        public BuildingActionConfig GetActionConfig()
        {
            return actionConfig;
        }

        public bool CanExecuteAction(string actionId)
        {
            if (actionConfig == null || string.IsNullOrEmpty(actionId))
                return false;

            var actionData = actionConfig.GetAction(actionId);
            if (actionData == null)
                return false;

            // Check action-specific requirements
            switch (actionId)
            {
                case "sell":
                    return IsConstructed; // Can only sell completed buildings

                case "upgrade":
                    // Check if this building implements IUpgradeable
                    if (this is IUpgradeable upgradeable)
                    {
                        return upgradeable.CanUpgrade();
                    }

                    return false;

                case "repair":
                    // Check if this building implements IRepairable
                    if (this is IRepairable repairable)
                    {
                        return repairable.Health < repairable.MaxHealth;
                    }

                    return false;

                default:
                    // For unit production actions, check if building is a producer
                    if (actionId.StartsWith("produce_"))
                    {
                        if (this is IUnitProducer producer)
                        {
                            // Would need UnitData to check properly
                            return IsConstructed;
                        }

                        return false;
                    }

                    // Default: action can be executed if building is constructed
                    return IsConstructed;
            }
        }

        public void ExecuteAction(string actionId)
        {
            if (!CanExecuteAction(actionId))
            {
                return;
            }

            // Delegate to BuildingActionExecutor
            if (BuildingActionExecutor.Instance != null)
            {
                BuildingActionExecutor.Instance.ExecuteAction(this, actionId);
            }
           
        }

        #endregion

        #region IUpgradeable Implementation

        public BuildingData GetUpgradeData()
        {
            return upgradeData;
        }

        public bool CanUpgrade()
        {
            // Can upgrade if:
            // 1. Building is constructed
            // 2. Not currently upgrading
            if (!IsConstructed || isUpgrading)
                return false;

            // TODO: Check if upgrade data is assigned (upgradeData != null)
            // TODO: Check if player has enough resources
            // For now, allow upgrades even without upgradeData set (for testing)
            return true;
        }

        public void StartUpgrade()
        {
            if (!CanUpgrade())
            {
                Debug.LogWarning($"Cannot start upgrade for {gameObject.name}");
                return;
            }

            isUpgrading = true;
            upgradeProgress = 0f;

            // TODO: Implement actual upgrade logic
            // - Deduct resources
            // - Start upgrade timer/progress
            // - Replace building prefab when complete
        }

        public float GetUpgradeProgress()
        {
            return isUpgrading ? upgradeProgress : 0f;
        }

        #endregion
    }
}

// Placeholder for IRepairable interface (to be implemented if health system is added)
namespace RTS.Interfaces
{
    public interface IRepairable
    {
        float Health { get; }
        float MaxHealth { get; }
        void Repair();
    }
}