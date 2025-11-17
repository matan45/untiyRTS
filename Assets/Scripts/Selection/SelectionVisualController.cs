using UnityEngine;
using RTS.Data;

namespace RTS.Selection
{
    /// <summary>
    /// Controls visual feedback for building selection (outline/highlight effect).
    /// Attach this component to buildings that should have selection visuals.
    /// Implements Single Responsibility Principle (SRP) - only handles selection visuals.
    /// </summary>
    public class SelectionVisualController : MonoBehaviour
    {
        [Header("Outline Settings")]
        [Tooltip("Default outline color if not specified in BuildingActionConfig")]
        [SerializeField] private Color defaultOutlineColor = new Color(0, 1, 0, 1); // Green

        [Tooltip("Default outline width if not specified in BuildingActionConfig")]
        [SerializeField] [Range(1f, 10f)] private float defaultOutlineWidth = 3f;

        [Header("References")]
        [Tooltip("Renderers to apply outline effect to (auto-detected if empty)")]
        [SerializeField] private Renderer[] targetRenderers;

        private Material[] originalMaterials;
        private Material[] outlineMaterials;
        private bool isShowingSelection = false;

        private void Awake()
        {
            // Auto-detect renderers if not manually assigned
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>();
            }

            CacheOriginalMaterials();
        }

        private void CacheOriginalMaterials()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                return;

            int totalMaterials = 0;
            foreach (var renderer in targetRenderers)
            {
                totalMaterials += renderer.sharedMaterials.Length;
            }

            originalMaterials = new Material[totalMaterials];
            int index = 0;

            foreach (var renderer in targetRenderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    originalMaterials[index++] = mat;
                }
            }
        }

        /// <summary>
        /// Shows the selection visual with configuration from BuildingActionConfig.
        /// </summary>
        /// <param name="config">Building action config containing selection visual settings</param>
        public void ShowSelection(BuildingActionConfig config)
        {
            if (isShowingSelection) return;

            Color outlineColor = defaultOutlineColor;
            float outlineWidth = defaultOutlineWidth;

            // Use config values if provided
            if (config != null)
            {
                outlineColor = config.selectionColor;
                outlineWidth = config.outlineWidth;
            }

            ApplyOutlineEffect(outlineColor, outlineWidth);
            isShowingSelection = true;
        }

        /// <summary>
        /// Hides the selection visual.
        /// </summary>
        public void HideSelection()
        {
            if (!isShowingSelection) return;

            RestoreOriginalMaterials();
            isShowingSelection = false;
        }

        private void ApplyOutlineEffect(Color color, float width)
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                return;

            // Simple color tint approach (works without custom shader)
            // For proper outline, you'd need a custom shader or post-processing
            foreach (var renderer in targetRenderers)
            {
                var materials = renderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    // Enable emission for glow effect
                    materials[i].EnableKeyword("_EMISSION");
                    materials[i].SetColor("_EmissionColor", color * 0.3f); // Subtle glow
                }

                renderer.materials = materials;
            }
        }

        private void RestoreOriginalMaterials()
        {
            if (targetRenderers == null || targetRenderers.Length == 0 || originalMaterials == null)
                return;

            // Disable emission
            foreach (var renderer in targetRenderers)
            {
                var materials = renderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].DisableKeyword("_EMISSION");
                    materials[i].SetColor("_EmissionColor", Color.black);
                }

                renderer.materials = materials;
            }
        }

        private void OnDestroy()
        {
            // Clean up material instances to prevent memory leaks
            if (isShowingSelection)
            {
                HideSelection();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test Show Selection")]
        private void TestShowSelection()
        {
            ShowSelection(null);
        }

        [ContextMenu("Test Hide Selection")]
        private void TestHideSelection()
        {
            HideSelection();
        }
#endif
    }
}
