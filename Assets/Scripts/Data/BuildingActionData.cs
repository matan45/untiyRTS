using UnityEngine;
using UnityEngine.InputSystem;

namespace RTS.Data
{
    /// <summary>
    /// ScriptableObject defining a single action that a building can perform.
    /// Examples: Sell, Upgrade, Produce Unit, Repair, etc.
    /// Implements Open/Closed Principle (OCP) - extend functionality by creating new data assets, not modifying code.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingAction", menuName = "RTS/Building Action", order = 1)]
    public class BuildingActionData : ScriptableObject
    {
        [Header("Action Identity")]
        [Tooltip("Unique identifier for this action (e.g., 'sell', 'upgrade', 'produce_infantry')")]
        public string actionId;

        [Header("Display")]
        [Tooltip("Display name shown in UI")]
        public string displayName;

        [Tooltip("Icon displayed on the action button")]
        public Sprite icon;

        [Header("Cost")]
        [Tooltip("Credit cost to perform this action (0 if free or gives resources)")]
        public int creditsCost;

        [Tooltip("Power required to perform this action")]
        public int powerCost;

        [Header("Input")]
        [Tooltip("Keyboard shortcut for this action")]
        public Key hotkey = Key.None;

        [Header("Visual Feedback")]
        [Tooltip("Color tint for the button (optional)")]
        public Color buttonTint = Color.white;

        /// <summary>
        /// Validates the action data in the editor.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(actionId))
            {
            }

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
            }
        }
    }
}
