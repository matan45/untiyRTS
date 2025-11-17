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

        [Tooltip("Color of the selection highlight")]
        public Color selectionColor = new Color(0, 1, 0, 0.5f); // Green with transparency

        [Tooltip("Outline width for selection effect (used by outline shader)")]
        [Range(1f, 10f)]
        public float outlineWidth = 3f;

        
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
}
