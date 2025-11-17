using UnityEngine;

namespace RTS.Data
{
    /// <summary>
    /// ScriptableObject container for all actions available to a specific building type.
    /// Linked to BuildingData to define what each building can do.
    /// Implements Data-Oriented Design - configuration separated from behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingActionConfig", menuName = "RTS/Building Action Config", order = 2)]
    public class BuildingActionConfig : ScriptableObject
    {
        [Header("Available Actions")]
        [Tooltip("List of all actions this building can perform")]
        public BuildingActionData[] actions;

        [Header("Selection Visual")]
        [Tooltip("Color of the selection highlight")]
        public Color selectionColor = new Color(0, 1, 0, 0.5f); // Green with transparency

        [Tooltip("Outline width for selection effect (used by outline shader)")]
        [Range(1f, 10f)]
        public float outlineWidth = 3f;

        [Header("UI Customization")]
        [Tooltip("Background color for the action panel")]
        public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        [Tooltip("Optional background sprite for the panel")]
        public Sprite panelBackgroundSprite;

        [Tooltip("Header color for building name/info section")]
        public Color headerColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        [Tooltip("Optional icon to display in the header")]
        public Sprite buildingIcon;

        [Tooltip("Accent color used for highlights and borders")]
        public Color accentColor = new Color(0, 1, 0, 1f);

        [Header("Button Layout")]
        [Tooltip("Layout type for action buttons")]
        public ButtonLayoutType layoutType = ButtonLayoutType.Grid;

        [Tooltip("Number of columns in grid layout (for Grid type)")]
        [Range(1, 6)]
        public int gridColumns = 3;

        [Tooltip("Spacing between buttons")]
        [Range(0f, 50f)]
        public float buttonSpacing = 10f;

        [Tooltip("Size of each button")]
        public Vector2 buttonSize = new Vector2(100f, 100f);

        [Tooltip("Padding around button container")]
        public Vector4 buttonContainerPadding = new Vector4(10f, 10f, 10f, 10f); // left, right, top, bottom

        
        public BuildingActionData GetAction(string actionId)
        {
            if (actions == null || string.IsNullOrEmpty(actionId))
                return null;

            return System.Array.Find(actions, a => a.actionId == actionId);
        }
        

        private void OnValidate()
        {
            if (actions != null && actions.Length > 0)
            {
                // Check for duplicate action IDs
                for (int i = 0; i < actions.Length; i++)
                {
                    if (actions[i] == null) continue;

                    for (int j = i + 1; j < actions.Length; j++)
                    {
                        if (actions[j] != null && actions[i].actionId == actions[j].actionId)
                        {
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Types of button layouts available for the action panel
    /// </summary>
    public enum ButtonLayoutType
    {
        Grid,           // Buttons arranged in a grid with specified columns
        Horizontal,     // Buttons arranged in a single row
        Vertical        // Buttons arranged in a single column
    }
}
