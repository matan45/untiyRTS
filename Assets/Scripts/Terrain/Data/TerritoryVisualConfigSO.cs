using UnityEngine;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// Configuration for territory ownership border visuals.
    /// Controls border appearance, materials, and rendering settings.
    /// </summary>
    [CreateAssetMenu(fileName = "TerritoryVisualConfig", menuName = "RTS/Terrain/Territory Visual Config")]
    public class TerritoryVisualConfigSO : ScriptableObject
    {
        [Header("Border Settings")]
        [Tooltip("Material for the border mesh (should support transparency)")]
        public Material borderMaterial;

        [Tooltip("Width of the territory border")]
        [Range(0.05f, 0.3f)]
        public float borderWidth = 0.12f;

        [Tooltip("Height offset above tile surface")]
        [Range(0.01f, 0.2f)]
        public float heightOffset = 0.03f;

        [Header("Appearance")]
        [Tooltip("Alpha/opacity of the border")]
        [Range(0.3f, 1f)]
        public float borderAlpha = 0.85f;

        [Header("Performance")]
        [Tooltip("Initial pool size for border objects")]
        public int initialPoolSize = 50;

        [Tooltip("Hex vertex count for border mesh (6 = flat edges)")]
        [Range(6, 12)]
        public int hexVertexCount = 6;

        /// <summary>
        /// Apply alpha to a color for border rendering.
        /// </summary>
        public Color ApplyAlpha(Color baseColor)
        {
            return new Color(baseColor.r, baseColor.g, baseColor.b, borderAlpha);
        }
    }
}
