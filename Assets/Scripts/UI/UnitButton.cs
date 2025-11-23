using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTS.Data;
using RTS.Buildings;

namespace RTS.UI
{
    /// <summary>
    /// UI button for selecting a unit to produce.
    /// Displays unit info and handles selection interaction.
    /// </summary>
    public class UnitButton : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private Button button;

        private UnitData unitData;
        private bool isUnlocked = false;

        public UnitData Data => unitData;

        private void Awake()
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

        /// <summary>
        /// Initialize button with unit data.
        /// </summary>
        public void Initialize(UnitData data)
        {
            unitData = data;
            UpdateUI();
        }

        /// <summary>
        /// Update button visual state based on current game state.
        /// </summary>
        public void UpdateUI()
        {
            if (unitData == null)
                return;

            // Update icon (if units have icons in the future)
            if (iconImage != null)
            {
                // UnitData doesn't have icon field yet, so we'll just disable it
                iconImage.enabled = false;
            }

            // Update name
            if (nameText != null)
                nameText.text = unitData.unitName;

            // Update cost
            if (costText != null)
                costText.text = $"${unitData.creditsCost}";

            // Check if unlocked
            UpdateLockState();

            // Check if affordable
            UpdateAffordableState();
        }

        /// <summary>
        /// Update lock state based on prerequisites.
        /// </summary>
        public void UpdateLockState()
        {
            BuildingManager buildingManager = FindFirstObjectByType<BuildingManager>();
            if (buildingManager == null || unitData == null)
                return;

            isUnlocked = unitData.HasPrerequisites(buildingManager);

            if (lockOverlay != null)
                lockOverlay.gameObject.SetActive(!isUnlocked);

            if (button != null)
                button.interactable = isUnlocked;
        }

        /// <summary>
        /// Update affordable state based on current resources.
        /// </summary>
        public void UpdateAffordableState()
        {
            ResourceManager resourceManager = FindFirstObjectByType<ResourceManager>();
            if (resourceManager == null || unitData == null)
                return;

            bool canAfford = resourceManager.CanAfford(
                unitData.creditsCost,
                unitData.powerCost
            );

            // Only update interactability if unlocked
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

        /// <summary>
        /// Called when button is clicked.
        /// </summary>
        private void OnButtonClicked()
        {
            if (unitData != null && UnitsMenu.Instance != null)
            {
                UnitsMenu.Instance.SelectUnit(unitData);
            }
        }
    }
}
