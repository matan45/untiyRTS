using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using RTS.Data;
using RTS.Interfaces;

namespace RTS.UI
{
    /// <summary>
    /// UI component for an individual building action button.
    /// Displays icon, name, cost, hotkey and handles click/hotkey input.
    /// Implements Single Responsibility Principle (SRP) - only manages one action button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BuildingActionButton : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hotkeyText;
        [SerializeField] private TextMeshProUGUI costText;

        [Header("Visual Feedback")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = Color.red;

        private BuildingActionData actionData;
        private IBuildingActions buildingActions;
        private InputAction hotkeyAction;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (button == null)
                button = GetComponent<Button>();

            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (nameText == null)
                nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

            if (hotkeyText == null)
                hotkeyText = transform.Find("HotkeyText")?.GetComponent<TextMeshProUGUI>();

            if (costText == null)
                costText = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Initializes this button with action data and the building that can perform the action.
        /// </summary>
        public void Initialize(BuildingActionData data, IBuildingActions building)
        {
            actionData = data;
            buildingActions = building;

            if (actionData == null)
            {
                Debug.LogWarning("BuildingActionButton initialized with null actionData!");
                gameObject.SetActive(false);
                return;
            }

            // Setup UI elements
            UpdateUI();

            // Setup button click listener
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }

            // Setup hotkey action (per CLAUDE.md: Use Input System callbacks instead of Update loop)
            SetupHotkeyAction();

            // Update initial state
            UpdateState();
        }

        private void SetupHotkeyAction()
        {
            // Clean up existing action
            if (hotkeyAction != null)
            {
                hotkeyAction.performed -= OnHotkeyPressed;
                hotkeyAction.Dispose();
                hotkeyAction = null;
            }

            // Create new action if hotkey is defined
            if (actionData != null && actionData.hotkey != Key.None)
            {
                hotkeyAction = new InputAction(
                    name: $"Hotkey_{actionData.actionId}",
                    binding: $"<Keyboard>/{actionData.hotkey}"
                );
                hotkeyAction.performed += OnHotkeyPressed;
                hotkeyAction.Enable();
            }
        }

        private void OnHotkeyPressed(InputAction.CallbackContext context)
        {
            if (button != null && button.interactable)
            {
                OnButtonClicked();
            }
        }

        private void OnDestroy()
        {
            // Clean up input action
            if (hotkeyAction != null)
            {
                hotkeyAction.performed -= OnHotkeyPressed;
                hotkeyAction.Dispose();
                hotkeyAction = null;
            }
        }

        private void UpdateUI()
        {
            if (actionData == null) return;

            // Set icon
            if (iconImage != null && actionData.icon != null)
            {
                iconImage.sprite = actionData.icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = actionData.displayName;
            }

            // Set hotkey
            if (hotkeyText != null)
            {
                if (actionData.hotkey != Key.None)
                {
                    hotkeyText.text = actionData.hotkey.ToString();
                    hotkeyText.enabled = true;
                }
                else
                {
                    hotkeyText.enabled = false;
                }
            }

            // Set cost
            if (costText != null)
            {
                if (actionData.creditsCost > 0 || actionData.powerCost > 0)
                {
                    string costString = "";

                    if (actionData.creditsCost > 0)
                        costString += $"${actionData.creditsCost}";

                    if (actionData.powerCost > 0)
                    {
                        if (costString.Length > 0) costString += " ";
                        costString += $"P:{actionData.powerCost}";
                    }

                    costText.text = costString;
                    costText.enabled = true;
                }
                else
                {
                    costText.enabled = false;
                }
            }

            // Apply button tint
            if (button != null && actionData.buttonTint != Color.white)
            {
                var colors = button.colors;
                colors.normalColor = actionData.buttonTint;
                button.colors = colors;
            }
        }

        /// <summary>
        /// Updates the button's interactable state based on whether the action can be executed.
        /// </summary>
        public void UpdateState()
        {
            if (actionData == null || buildingActions == null || button == null)
                return;

            bool canExecute = buildingActions.CanExecuteAction(actionData.actionId);
            button.interactable = canExecute;

            // Update cost text color based on affordability
            if (costText != null && costText.enabled)
            {
                costText.color = canExecute ? affordableColor : unaffordableColor;
            }
        }

        private void OnButtonClicked()
        {
            if (actionData == null || buildingActions == null)
                return;

            // Execute the action
            buildingActions.ExecuteAction(actionData.actionId);
        }

#if UNITY_EDITOR
        [ContextMenu("Find UI Elements")]
        private void FindUIElements()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (nameText == null)
                nameText = GetComponentInChildren<TextMeshProUGUI>();

        }
#endif
    }
}
