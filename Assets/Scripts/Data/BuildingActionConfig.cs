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
        [Tooltip("Sprite used for selection ring (optional, can use shader instead)")]
        public Sprite selectionRingSprite;

        [Tooltip("Color of the selection highlight")]
        public Color selectionColor = new Color(0, 1, 0, 0.5f); // Green with transparency

        [Tooltip("Outline width for selection effect (used by outline shader)")]
        [Range(1f, 10f)]
        public float outlineWidth = 3f;

        /// <summary>
        /// Gets an action by its unique ID.
        /// </summary>
        /// <param name="actionId">The action identifier to search for</param>
        /// <returns>The BuildingActionData if found, null otherwise</returns>
        public BuildingActionData GetAction(string actionId)
        {
            if (actions == null || string.IsNullOrEmpty(actionId))
                return null;

            return System.Array.Find(actions, a => a.actionId == actionId);
        }

        /// <summary>
        /// Checks if this config contains a specific action.
        /// </summary>
        /// <param name="actionId">The action identifier to check</param>
        /// <returns>True if the action exists in this config</returns>
        public bool HasAction(string actionId)
        {
            return GetAction(actionId) != null;
        }

        /// <summary>
        /// Gets all action IDs for quick lookup.
        /// </summary>
        public string[] GetActionIds()
        {
            if (actions == null) return new string[0];

            string[] ids = new string[actions.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                ids[i] = actions[i] != null ? actions[i].actionId : null;
            }
            return ids;
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
                            Debug.LogWarning($"Duplicate action ID '{actions[i].actionId}' found in {name}!", this);
                        }
                    }
                }
            }
        }
    }
}
