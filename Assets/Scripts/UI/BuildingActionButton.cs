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

            // Update initial state
            UpdateState();
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
                if (actionData.hotkey != KeyCode.None)
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
                        costString += $"⚡{actionData.powerCost}";
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

            Debug.Log($"Executed action: {actionData.displayName}");
        }

        private void Update()
        {
            // Check for hotkey press
            if (actionData != null && actionData.hotkey != KeyCode.None)
            {
                if (Keyboard.current != null)
                {
                    var key = ConvertKeyCodeToKey(actionData.hotkey);
                    if (key != Key.None && Keyboard.current[key].wasPressedThisFrame)
                    {
                        if (button != null && button.interactable)
                        {
                            OnButtonClicked();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts legacy KeyCode to new Input System Key.
        /// </summary>
        private Key ConvertKeyCodeToKey(KeyCode keyCode)
        {
            // Simple conversion for common keys
            switch (keyCode)
            {
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;
                default: return Key.None;
            }
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

            Debug.Log("UI elements search complete");
        }
#endif
    }
}
