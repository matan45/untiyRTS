using UnityEngine;
using System.Collections.Generic;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// Configuration for terrain movement properties.
    /// Allows designers to tune movement costs without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainMovementConfig", menuName = "RTS/Terrain/Movement Config")]
    public class TerrainMovementConfigSO : ScriptableObject
    {
        [System.Serializable]
        public class TerrainMovementData
        {
            [Tooltip("The terrain type this configuration applies to")]
            public TerrainType terrainType;

            [Tooltip("Movement cost multiplier (1 = normal, 2 = double cost)")]
            [Range(0.5f, 10f)]
            public float movementCost = 1f;

            [Tooltip("Can ground units traverse this terrain?")]
            public bool isPassable = true;
        }

        [SerializeField]
        [Tooltip("Movement configuration for each terrain type")]
        private TerrainMovementData[] terrainData;

        private Dictionary<TerrainType, TerrainMovementData> _lookup;

        /// <summary>
        /// Get movement cost for a terrain type.
        /// Returns float.MaxValue if impassable.
        /// </summary>
        /// <param name="type">The terrain type to query</param>
        /// <returns>Movement cost multiplier, or float.MaxValue if impassable</returns>
        public float GetMovementCost(TerrainType type)
        {
            EnsureLookup();
            if (_lookup.TryGetValue(type, out var data))
            {
                return data.isPassable ? data.movementCost : float.MaxValue;
            }
            return 1f; // Default cost
        }

        /// <summary>
        /// Check if terrain is passable.
        /// </summary>
        /// <param name="type">The terrain type to query</param>
        /// <returns>True if passable, false otherwise</returns>
        public bool IsPassable(TerrainType type)
        {
            EnsureLookup();
            if (_lookup.TryGetValue(type, out var data))
            {
                return data.isPassable;
            }
            return true; // Default passable
        }

        /// <summary>
        /// Ensures the lookup dictionary is initialized.
        /// Uses lazy initialization for performance.
        /// </summary>
        private void EnsureLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<TerrainType, TerrainMovementData>();
            if (terrainData == null) return;

            foreach (var data in terrainData)
            {
                _lookup[data.terrainType] = data;
            }
        }

        /// <summary>
        /// Called when values are changed in the inspector.
        /// Forces rebuild of lookup on next access.
        /// </summary>
        private void OnValidate()
        {
            _lookup = null;
        }
    }
}
