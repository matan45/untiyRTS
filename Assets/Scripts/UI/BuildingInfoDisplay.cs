using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTS.Buildings;
using RTS.Interfaces;

namespace RTS.UI
{
    /// <summary>
    /// Displays information about the currently selected building.
    /// Shows: Building name, icon, construction progress, health, and status.
    /// Implements Single Responsibility Principle (SRP) - only displays building info.
    /// </summary>
    public class BuildingInfoDisplay : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private Image buildingIconImage;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Image constructionBarFill;
        [SerializeField] private GameObject constructionBarObject;

        [Header("Display Settings")]
        [SerializeField] private bool showConstructionProgress = true;
        [SerializeField] private bool showHealth = true;

        private Building currentBuilding;

        private void Awake()
        {
            // Auto-find UI elements if not assigned
            if (buildingNameText == null)
                buildingNameText = transform.Find("BuildingNameText")?.GetComponent<TextMeshProUGUI>();

            if (buildingIconImage == null)
                buildingIconImage = transform.Find("BuildingIcon")?.GetComponent<Image>();

            if (statusText == null)
                statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

            if (healthBarFill == null)
                healthBarFill = transform.Find("HealthBarBackground/HealthBarFill")?.GetComponent<Image>();

            if (constructionBarFill == null)
                constructionBarFill = transform.Find("ConstructionBarBackground/ConstructionBarFill")?.GetComponent<Image>();

            if (constructionBarObject == null)
                constructionBarObject = transform.Find("ConstructionBarBackground")?.gameObject;
        }

        /// <summary>
        /// Updates the display with information from the specified building.
        /// </summary>
        public void UpdateDisplay(Building building)
        {
            currentBuilding = building;

            if (building == null || building.Data == null)
            {
                ClearDisplay();
                return;
            }

            // Update building name
            if (buildingNameText != null)
            {
                buildingNameText.text = building.Data.buildingName;
            }

            // Update building icon
            if (buildingIconImage != null && building.Data.icon != null)
            {
                buildingIconImage.sprite = building.Data.icon;
                buildingIconImage.enabled = true;
            }
            else if (buildingIconImage != null)
            {
                buildingIconImage.enabled = false;
            }

            // Update status
            UpdateStatus();

            // Update construction progress
            UpdateConstructionProgress();

            // Update health
            UpdateHealth();
        }

        /// <summary>
        /// Clears all displayed information.
        /// </summary>
        public void ClearDisplay()
        {
            currentBuilding = null;

            if (buildingNameText != null)
                buildingNameText.text = "";

            if (buildingIconImage != null)
                buildingIconImage.enabled = false;

            if (statusText != null)
                statusText.text = "";

            if (constructionBarObject != null)
                constructionBarObject.SetActive(false);

            if (healthBarFill != null)
                healthBarFill.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Update info every frame if a building is selected
            if (currentBuilding != null)
            {
                UpdateStatus();
                UpdateConstructionProgress();
                UpdateHealth();
            }
        }

        private void UpdateStatus()
        {
            if (statusText == null || currentBuilding == null)
                return;

            string status = "";

            if (!currentBuilding.IsConstructed)
            {
                status = "Under Construction";
            }
            else
            {
                // Check for special states
                if (currentBuilding is IUpgradeable upgradeable)
                {
                    float upgradeProgress = upgradeable.GetUpgradeProgress();
                    if (upgradeProgress > 0f && upgradeProgress < 1f)
                    {
                        status = $"Upgrading ({Mathf.FloorToInt(upgradeProgress * 100)}%)";
                    }
                }

                if (string.IsNullOrEmpty(status))
                {
                    status = "Operational";
                }
            }

            statusText.text = status;
        }

        private void UpdateConstructionProgress()
        {
            if (!showConstructionProgress || constructionBarFill == null || currentBuilding == null)
            {
                if (constructionBarObject != null)
                    constructionBarObject.SetActive(false);
                return;
            }

            if (!currentBuilding.IsConstructed)
            {
                if (constructionBarObject != null)
                    constructionBarObject.SetActive(true);
                float progress = currentBuilding.GetConstructionProgress();
                constructionBarFill.fillAmount = progress;
            }
            else
            {
                if (constructionBarObject != null)
                    constructionBarObject.SetActive(false);
            }
        }

        private void UpdateHealth()
        {
            if (!showHealth || healthBarFill == null || currentBuilding == null)
            {
                if (healthBarFill != null)
                    healthBarFill.gameObject.SetActive(false);
                return;
            }

            // Check if building has health (implements IRepairable)
            if (currentBuilding is IRepairable repairable)
            {
                healthBarFill.gameObject.SetActive(true);
                healthBarFill.fillAmount = repairable.Health / repairable.MaxHealth;

                // Change color based on health
                float healthPercent = repairable.Health / repairable.MaxHealth;

                if (healthPercent > 0.6f)
                    healthBarFill.color = Color.green;
                else if (healthPercent > 0.3f)
                    healthBarFill.color = Color.yellow;
                else
                    healthBarFill.color = Color.red;
            }
            else
            {
                // Building doesn't have health system
                healthBarFill.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Find UI Elements")]
        private void FindUIElements()
        {
            if (buildingNameText == null)
                buildingNameText = transform.Find("BuildingName")?.GetComponent<TextMeshProUGUI>();

            if (buildingIconImage == null)
                buildingIconImage = transform.Find("BuildingIcon")?.GetComponent<Image>();

            if (statusText == null)
                statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

            if (healthBarFill == null)
                healthBarFill = transform.Find("HealthBarBackground/HealthBarFill")?.GetComponent<Image>();

            if (constructionBarFill == null)
                constructionBarFill = transform.Find("ConstructionBarBackground/ConstructionBarFill")?.GetComponent<Image>();

            if (constructionBarObject == null)
                constructionBarObject = transform.Find("ConstructionBarBackground")?.gameObject;

            Debug.Log("UI elements search complete");
        }
#endif
    }
}
