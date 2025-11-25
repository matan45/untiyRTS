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

        private Material[][] originalMaterials; // Store original materials per renderer
        private Material[][] instancedMaterials; // Store material instances per renderer
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

            originalMaterials = new Material[targetRenderers.Length][];

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                originalMaterials[i] = targetRenderers[i].sharedMaterials;
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

            // Create and store material instances
            instancedMaterials = new Material[targetRenderers.Length][];

            for (int r = 0; r < targetRenderers.Length; r++)
            {
                var renderer = targetRenderers[r];
                var materials = renderer.materials; // Creates new instances - store them!
                instancedMaterials[r] = materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    // Enable emission for glow effect
                    materials[i].EnableKeyword("_EMISSION");
                    materials[i].SetColor("_EmissionColor", color * 0.3f);
                }

                renderer.materials = materials;
            }
        }

        private void RestoreOriginalMaterials()
        {
            if (targetRenderers == null || targetRenderers.Length == 0 || originalMaterials == null)
                return;

            // Restore shared materials
            for (int r = 0; r < targetRenderers.Length; r++)
            {
                if (r < originalMaterials.Length && originalMaterials[r] != null)
                {
                    targetRenderers[r].sharedMaterials = originalMaterials[r];
                }
            }

            // Destroy material instances to prevent memory leak
            if (instancedMaterials != null)
            {
                foreach (var materials in instancedMaterials)
                {
                    if (materials != null)
                    {
                        foreach (var mat in materials)
                        {
                            if (mat != null)
                                Destroy(mat);
                        }
                    }
                }
                instancedMaterials = null;
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
    }
}
