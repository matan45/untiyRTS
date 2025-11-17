using UnityEngine;
using RTS.Interfaces;
using RTS.Data;
using RTS.Selection;
using RTS.Actions;

namespace RTS.Buildings
{
    public class Building : MonoBehaviour, ISelectable, IBuildingActions, IUpgradeable
    {
        [Header("Building Configuration")]
        [SerializeField] private BuildingData buildingData;
        [SerializeField] private BuildingActionConfig actionConfig;

        [Header("Upgrade Configuration")]
        [SerializeField] private BuildingData upgradeData;

        [Header("Selection Visual")]
        [SerializeField] private SelectionVisualController selectionVisual;

        public BuildingData Data => buildingData;
        public bool IsConstructed { get; private set; }

        // ISelectable implementation
        public bool IsSelected { get; private set; }
        public GameObject GameObject => gameObject;

        private float constructionProgress = 0f;
        private bool isUpgrading = false;
        private float upgradeProgress = 0f;
    
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

            Debug.Log($"Building {gameObject.name} selected");
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

            Debug.Log($"Building {gameObject.name} deselected");
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
                Debug.LogWarning($"Cannot execute action '{actionId}' on {gameObject.name}");
                return;
            }

            // Delegate to BuildingActionExecutor
            if (BuildingActionExecutor.Instance != null)
            {
                BuildingActionExecutor.Instance.ExecuteAction(this, actionId);
            }
            else
            {
                Debug.LogError("BuildingActionExecutor.Instance is null! Cannot execute action.");
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
            Debug.Log($"Started upgrade for {gameObject.name}");

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