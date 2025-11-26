using UnityEngine;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// ScriptableObject defining properties for a terrain type.
    /// Create one asset per terrain type for designer-friendly configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainType", menuName = "RTS/Terrain/Terrain Type Data", order = 1)]
    public class TerrainTypeDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("The terrain type this data represents")]
        public TerrainType terrainType;

        [Tooltip("Display name shown in UI")]
        public string displayName;

        [Tooltip("Description shown in tooltips")]
        [TextArea(2, 4)]
        public string description;

        [Header("Biome Classification")]
        [Tooltip("Biome category for this terrain type")]
        public BiomeType biome;

        [Header("Gameplay Properties")]
        [Tooltip("Movement cost multiplier (1.0 = normal, higher = slower)")]
        [Range(0.5f, 10f)]
        public float movementCost = 1f;

        [Tooltip("Whether buildings can be placed on this terrain")]
        public bool isBuildable = true;

        [Tooltip("Whether units can pass through this terrain")]
        public bool isPassable = true;

        [Tooltip("Defense bonus/penalty for units on this terrain")]
        [Range(-5, 5)]
        public int defenseBonus = 0;

        [Header("Visual Properties")]
        [Tooltip("Default material for this terrain type")]
        public Material defaultMaterial;

        [Tooltip("Color shown on minimap")]
        public Color minimapColor = Color.white;

        [Tooltip("Base height offset for terrain generation")]
        public float baseHeight = 0f;

        [Header("Resource Generation")]
        [Tooltip("Possible resources this terrain can yield")]
        public TileResourceYield[] possibleResources;

        /// <summary>
        /// Check if this terrain can yield a specific resource type.
        /// </summary>
        public bool CanYieldResource(ResourceType resourceType)
        {
            if (possibleResources == null) return false;

            foreach (var resource in possibleResources)
            {
                if (resource.resourceType == resourceType)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the base yield for a specific resource type.
        /// Returns 0 if this terrain doesn't yield that resource.
        /// </summary>
        public int GetBaseYield(ResourceType resourceType)
        {
            if (possibleResources == null) return 0;

            foreach (var resource in possibleResources)
            {
                if (resource.resourceType == resourceType)
                    return resource.baseYield;
            }
            return 0;
        }

        /// <summary>
        /// Get the total resource potential (sum of all base yields).
        /// </summary>
        public int GetTotalResourcePotential()
        {
            if (possibleResources == null) return 0;

            int total = 0;
            foreach (var resource in possibleResources)
            {
                total += resource.baseYield;
            }
            return total;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-fill display name from asset name if empty
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = terrainType.ToString();
            }
        }
#endif
    }
}
