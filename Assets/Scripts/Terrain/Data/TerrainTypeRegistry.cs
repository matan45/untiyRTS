using System.Collections.Generic;
using UnityEngine;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// Registry that provides quick lookup of TerrainTypeDataSO by TerrainType enum.
    /// Create a single instance of this asset and populate it with all terrain types.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainTypeRegistry", menuName = "RTS/Terrain/Terrain Type Registry", order = 0)]
    public class TerrainTypeRegistry : ScriptableObject
    {
        [Tooltip("Array of all terrain type configurations. Order doesn't matter.")]
        [SerializeField]
        private TerrainTypeDataSO[] terrainTypes;

        private Dictionary<TerrainType, TerrainTypeDataSO> _lookup;
        private bool _isInitialized;

        /// <summary>
        /// Initialize the lookup dictionary. Called automatically on first access.
        /// </summary>
        public void Initialize()
        {
            _lookup = new Dictionary<TerrainType, TerrainTypeDataSO>();

            if (terrainTypes == null)
            {
                Debug.LogWarning($"TerrainTypeRegistry '{name}' has no terrain types configured.");
                _isInitialized = true;
                return;
            }

            foreach (var data in terrainTypes)
            {
                if (data == null)
                {
                    Debug.LogWarning($"TerrainTypeRegistry '{name}' contains null entry.");
                    continue;
                }

                if (_lookup.ContainsKey(data.terrainType))
                {
                    Debug.LogWarning($"TerrainTypeRegistry '{name}' contains duplicate entry for {data.terrainType}.");
                    continue;
                }

                _lookup[data.terrainType] = data;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Get configuration for a specific terrain type.
        /// Returns null if not found.
        /// </summary>
        public TerrainTypeDataSO GetConfig(TerrainType type)
        {
            if (!_isInitialized)
                Initialize();

            return _lookup.TryGetValue(type, out var config) ? config : null;
        }

        /// <summary>
        /// Check if a terrain type has a configuration.
        /// </summary>
        public bool HasConfig(TerrainType type)
        {
            if (!_isInitialized)
                Initialize();

            return _lookup.ContainsKey(type);
        }

        /// <summary>
        /// Get all configured terrain types.
        /// </summary>
        public IEnumerable<TerrainTypeDataSO> GetAllConfigs()
        {
            if (!_isInitialized)
                Initialize();

            return _lookup.Values;
        }

        /// <summary>
        /// Get all terrain types that belong to a specific biome.
        /// </summary>
        public IEnumerable<TerrainTypeDataSO> GetConfigsByBiome(BiomeType biome)
        {
            if (!_isInitialized)
                Initialize();

            foreach (var config in _lookup.Values)
            {
                if (config.biome == biome)
                    yield return config;
            }
        }

        /// <summary>
        /// Get all buildable terrain types.
        /// </summary>
        public IEnumerable<TerrainTypeDataSO> GetBuildableConfigs()
        {
            if (!_isInitialized)
                Initialize();

            foreach (var config in _lookup.Values)
            {
                if (config.isBuildable)
                    yield return config;
            }
        }

        /// <summary>
        /// Get the number of configured terrain types.
        /// </summary>
        public int Count
        {
            get
            {
                if (!_isInitialized)
                    Initialize();

                return _lookup.Count;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Reset initialization when values change in editor
            _isInitialized = false;
            _lookup = null;

            // Check for missing terrain types
            if (terrainTypes != null)
            {
                var configuredTypes = new HashSet<TerrainType>();
                foreach (var data in terrainTypes)
                {
                    if (data != null)
                        configuredTypes.Add(data.terrainType);
                }

                var allTypes = System.Enum.GetValues(typeof(TerrainType));
                foreach (TerrainType type in allTypes)
                {
                    if (!configuredTypes.Contains(type))
                    {
                        Debug.LogWarning($"TerrainTypeRegistry '{name}' is missing configuration for {type}.");
                    }
                }
            }
        }
#endif
    }
}
