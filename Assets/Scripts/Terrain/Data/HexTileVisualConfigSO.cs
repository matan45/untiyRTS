using UnityEngine;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// ScriptableObject for configuring hex tile selection visuals.
    /// Provides settings for selection overlay, hover preview, and animations.
    /// </summary>
    [CreateAssetMenu(fileName = "HexTileVisualConfig", menuName = "RTS/Terrain/Hex Tile Visual Config")]
    public class HexTileVisualConfigSO : ScriptableObject
    {
        [Header("Selection Overlay")]
        [Tooltip("Material for the selection overlay mesh")]
        public Material selectionOverlayMaterial;

        [Tooltip("Color tint for selection overlay")]
        public Color selectionColor = new Color(1f, 1f, 0f, 0.3f);

        [Tooltip("Height offset above tile surface for overlay")]
        [Range(0.01f, 0.5f)]
        public float overlayHeightOffset = 0.05f;

        [Header("Hover Preview")]
        [Tooltip("Material for hover preview overlay (optional, uses selection if null)")]
        public Material hoverOverlayMaterial;

        [Tooltip("Color tint for hover preview")]
        public Color hoverColor = new Color(1f, 1f, 1f, 0.2f);

        [Header("Pulse Animation")]
        [Tooltip("Enable pulse animation on selection")]
        public bool enablePulse = true;

        [Tooltip("Speed of pulse animation")]
        [Range(0.5f, 5f)]
        public float pulseSpeed = 2f;

        [Tooltip("Alpha range for pulse animation (min, max)")]
        public Vector2 pulseAlphaRange = new Vector2(0.2f, 0.4f);

        [Header("Tile Mesh Settings")]
        [Tooltip("Default tile height (cylinder thickness)")]
        [Range(0.1f, 1f)]
        public float defaultTileHeight = 0.3f;

        [Tooltip("Number of vertices around hex circumference (6 = flat edges, higher = smoother)")]
        [Range(6, 36)]
        public int hexVertexCount = 6;

        [Header("Bevel Settings")]
        [Tooltip("Size of the beveled edge on tile corners")]
        [Range(0f, 0.15f)]
        public float bevelSize = 0.05f;

        [Header("Border Settings")]
        [Tooltip("Width of the border/outline on the top surface")]
        [Range(0f, 0.2f)]
        public float borderWidth = 0.08f;

        [Header("Top Surface Detail")]
        [Tooltip("Depth of the center depression on the top surface")]
        [Range(0f, 0.1f)]
        public float centerDepth = 0.02f;

        /// <summary>
        /// Get the effective hover material (falls back to selection material).
        /// </summary>
        public Material GetHoverMaterial()
        {
            return hoverOverlayMaterial != null ? hoverOverlayMaterial : selectionOverlayMaterial;
        }

        /// <summary>
        /// Calculate current pulse alpha value based on time.
        /// </summary>
        public float GetPulseAlpha(float time)
        {
            if (!enablePulse) return selectionColor.a;

            float t = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f;
            return Mathf.Lerp(pulseAlphaRange.x, pulseAlphaRange.y, t);
        }
    }
}
