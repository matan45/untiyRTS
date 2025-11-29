using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.Terrain;
using RTS.Terrain.Data;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Represents a single hexagonal tile in the grid.
    /// Tracks occupancy, terrain type, ownership, resources, and world position.
    /// </summary>
    public class HexTile
    {
        #region Existing Properties (Backward Compatible)

        public Vector2Int Coordinates { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public TerrainType TerrainType { get; set; }
        public bool IsBuildable { get; set; }
        public GameObject OccupyingBuilding { get; private set; }

        #endregion

        #region New Properties

        /// <summary>
        /// Reference to the terrain configuration ScriptableObject.
        /// </summary>
        public TerrainTypeDataSO TerrainConfig { get; private set; }

        /// <summary>
        /// Owner player ID. -1 means unowned/neutral.
        /// </summary>
        public int OwnerId { get; private set; } = -1;

        /// <summary>
        /// Check if this tile has an owner.
        /// </summary>
        public bool HasOwner => OwnerId >= 0;

        /// <summary>
        /// Event fired when ownership changes.
        /// Parameters: previousOwnerId, newOwnerId
        /// </summary>
        public event Action<int, int> OnOwnershipChanged;

        /// <summary>
        /// Event fired when visibility changes for a player.
        /// Parameters: playerId, isVisible
        /// </summary>
        public event Action<int, bool> OnVisibilityChanged;

        /// <summary>
        /// Building ID for serialization purposes. -1 means no building.
        /// </summary>
        public int OccupyingBuildingId { get; private set; } = -1;

        // Per-player visibility states for Fog of War
        private Dictionary<int, PlayerVisibilityState> _playerVisibility = new Dictionary<int, PlayerVisibilityState>();

        // Resources dictionary for flexible resource tracking
        private Dictionary<ResourceType, TileResourceValue> _resources = new Dictionary<ResourceType, TileResourceValue>();

        #endregion

        #region Computed Properties from TerrainConfig

        /// <summary>
        /// Movement cost for traversing this tile. Higher = slower.
        /// </summary>
        public float MovementCost => TerrainConfig?.movementCost ?? 1f;

        /// <summary>
        /// Whether units can pass through this tile.
        /// </summary>
        public bool IsPassable => TerrainConfig?.isPassable ?? true;

        /// <summary>
        /// Defense bonus/penalty for units on this tile.
        /// </summary>
        public int DefenseBonus => TerrainConfig?.defenseBonus ?? 0;

        /// <summary>
        /// Biome category of this tile.
        /// </summary>
        public BiomeType Biome => TerrainConfig?.biome ?? BiomeType.Temperate;

        #endregion

        #region Constructors

        public HexTile(Vector2Int coordinates, Vector3 worldPosition)
        {
            Coordinates = coordinates;
            WorldPosition = worldPosition;
            TerrainType = TerrainType.Grassland;
            IsBuildable = true;
            OccupyingBuilding = null;
            OwnerId = -1;
            OccupyingBuildingId = -1;
            _resources = new Dictionary<ResourceType, TileResourceValue>();
            _playerVisibility = new Dictionary<int, PlayerVisibilityState>();
        }

        public HexTile(int q, int r, Vector3 worldPosition)
            : this(new Vector2Int(q, r), worldPosition)
        {
        }

        #endregion

        #region Terrain Configuration

        /// <summary>
        /// Set the terrain configuration reference.
        /// Updates IsBuildable based on config.
        /// </summary>
        public void SetTerrainConfig(TerrainTypeDataSO config)
        {
            TerrainConfig = config;
            if (config != null)
            {
                IsBuildable = config.isBuildable && !IsOccupied;
            }
        }

        #endregion

        #region Ownership Methods

        /// <summary>
        /// Set the owner of this tile.
        /// </summary>
        /// <param name="playerId">Player ID to set as owner. Use -1 for neutral.</param>
        public void SetOwner(int playerId)
        {
            if (OwnerId == playerId) return; // No change, avoid event spam

            int previousOwner = OwnerId;
            OwnerId = playerId;
            OnOwnershipChanged?.Invoke(previousOwner, playerId);
        }

        /// <summary>
        /// Clear ownership of this tile (set to neutral).
        /// </summary>
        public void ClearOwner()
        {
            if (OwnerId == -1) return; // Already neutral

            int previousOwner = OwnerId;
            OwnerId = -1;
            OnOwnershipChanged?.Invoke(previousOwner, -1);
        }

        /// <summary>
        /// Check if this tile is owned by a specific player.
        /// </summary>
        public bool IsOwnedBy(int playerId)
        {
            return OwnerId == playerId;
        }

        #endregion

        #region Fog of War / Visibility Methods

        /// <summary>
        /// Initialize visibility for a player. Call this for each player at game start.
        /// </summary>
        /// <param name="playerId">Player ID to initialize</param>
        /// <param name="visible">Initial visibility state (default: true for RTS-19)</param>
        /// <param name="explored">Initial exploration state (default: true for RTS-19)</param>
        public void InitializeVisibilityForPlayer(int playerId, bool visible = true, bool explored = true)
        {
            _playerVisibility[playerId] = new PlayerVisibilityState(playerId, visible, explored);
        }

        /// <summary>
        /// Check if this tile is currently visible to a player.
        /// Returns true if player has no visibility state (default: visible).
        /// </summary>
        public bool IsVisibleToPlayer(int playerId)
        {
            return !_playerVisibility.TryGetValue(playerId, out var state) || state.isVisible;
        }

        /// <summary>
        /// Check if this tile has been explored by a player.
        /// Returns true if player has no visibility state (default: explored).
        /// </summary>
        public bool IsExploredByPlayer(int playerId)
        {
            return !_playerVisibility.TryGetValue(playerId, out var state) || state.hasBeenExplored;
        }

        /// <summary>
        /// Set visibility state for a player.
        /// Automatically marks as explored when becoming visible.
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="isVisible">Whether tile is currently visible</param>
        public void SetVisibility(int playerId, bool isVisible)
        {
            if (!_playerVisibility.TryGetValue(playerId, out var state))
            {
                state = new PlayerVisibilityState(playerId, isVisible, isVisible);
                _playerVisibility[playerId] = state;
                OnVisibilityChanged?.Invoke(playerId, isVisible);
            }
            else if (state.isVisible != isVisible)
            {
                state.isVisible = isVisible;
                if (isVisible)
                {
                    state.hasBeenExplored = true;
                }
                OnVisibilityChanged?.Invoke(playerId, isVisible);
            }
        }

        /// <summary>
        /// Mark this tile as explored by a player.
        /// </summary>
        public void MarkExplored(int playerId)
        {
            if (!_playerVisibility.TryGetValue(playerId, out var state))
            {
                state = new PlayerVisibilityState(playerId, false, true);
                _playerVisibility[playerId] = state;
            }
            else
            {
                state.hasBeenExplored = true;
            }
        }

        /// <summary>
        /// Get visibility state for a player.
        /// Returns null if player has no visibility state.
        /// </summary>
        public PlayerVisibilityState GetVisibilityState(int playerId)
        {
            return _playerVisibility.TryGetValue(playerId, out var state) ? state : null;
        }

        /// <summary>
        /// Get visibility states as array for serialization.
        /// </summary>
        public PlayerVisibilityState[] GetVisibilityAsArray()
        {
            var result = new PlayerVisibilityState[_playerVisibility.Count];
            int i = 0;
            foreach (var state in _playerVisibility.Values)
            {
                result[i++] = state.Clone();
            }
            return result;
        }

        /// <summary>
        /// Load visibility states from array (for deserialization).
        /// </summary>
        public void LoadVisibility(PlayerVisibilityState[] states)
        {
            _playerVisibility.Clear();
            if (states != null)
            {
                foreach (var state in states)
                {
                    if (state != null)
                    {
                        _playerVisibility[state.playerId] = state;
                    }
                }
            }
        }

        #endregion

        #region Resource Methods

        /// <summary>
        /// Get the current amount of a specific resource.
        /// </summary>
        public int GetResourceAmount(ResourceType type)
        {
            return _resources.TryGetValue(type, out var value) ? value.currentAmount : 0;
        }

        /// <summary>
        /// Get the maximum capacity for a specific resource.
        /// </summary>
        public int GetResourceMax(ResourceType type)
        {
            return _resources.TryGetValue(type, out var value) ? value.maxAmount : 0;
        }

        /// <summary>
        /// Set the resource amount for a specific type.
        /// </summary>
        /// <param name="type">Resource type</param>
        /// <param name="amount">Current amount</param>
        /// <param name="max">Maximum capacity (-1 to keep existing)</param>
        public void SetResourceAmount(ResourceType type, int amount, int max = -1)
        {
            if (!_resources.ContainsKey(type))
            {
                _resources[type] = new TileResourceValue
                {
                    resourceType = type,
                    currentAmount = amount,
                    maxAmount = max >= 0 ? max : amount
                };
            }
            else
            {
                _resources[type].currentAmount = amount;
                if (max >= 0)
                    _resources[type].maxAmount = max;
            }
        }

        /// <summary>
        /// Attempt to harvest a resource from this tile.
        /// </summary>
        /// <returns>True if harvest was successful.</returns>
        public bool HarvestResource(ResourceType type, int amount)
        {
            if (!_resources.TryGetValue(type, out var value))
                return false;

            if (value.currentAmount < amount)
                return false;

            value.currentAmount -= amount;
            return true;
        }

        /// <summary>
        /// Add resources to this tile.
        /// </summary>
        /// <returns>Amount actually added (may be less if at capacity).</returns>
        public int AddResource(ResourceType type, int amount)
        {
            if (!_resources.TryGetValue(type, out var value))
            {
                _resources[type] = new TileResourceValue
                {
                    resourceType = type,
                    currentAmount = amount,
                    maxAmount = amount
                };
                return amount;
            }

            return value.Add(amount);
        }

        /// <summary>
        /// Check if this tile has any resources.
        /// </summary>
        public bool HasResources => _resources.Count > 0 && _resources.Values.Any(r => r.currentAmount > 0);

        /// <summary>
        /// Check if this tile has a specific resource type.
        /// </summary>
        public bool HasResourceType(ResourceType type)
        {
            return _resources.ContainsKey(type) && _resources[type].currentAmount > 0;
        }

        /// <summary>
        /// Get all resource types present on this tile.
        /// </summary>
        public IEnumerable<ResourceType> GetResourceTypes()
        {
            return _resources.Keys;
        }

        /// <summary>
        /// Get the resources dictionary for direct iteration (non-allocating).
        /// Use foreach (var kvp in tile.Resources) for zero-allocation iteration.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, TileResourceValue> Resources => _resources;

        /// <summary>
        /// Get resources as array for serialization.
        /// </summary>
        public TileResourceValue[] GetResourcesAsArray()
        {
            return _resources.Values.ToArray();
        }

        /// <summary>
        /// Load resources from array (for deserialization).
        /// </summary>
        public void LoadResources(TileResourceValue[] resources)
        {
            _resources.Clear();
            if (resources != null)
            {
                foreach (var r in resources)
                {
                    if (r != null)
                        _resources[r.resourceType] = r;
                }
            }
        }

        /// <summary>
        /// Clear all resources from this tile.
        /// </summary>
        public void ClearResources()
        {
            _resources.Clear();
        }

        #endregion

        #region Building Occupancy (Existing + Enhanced)

        /// <summary>
        /// Mark this tile as occupied by a building.
        /// </summary>
        public void SetOccupyingBuilding(GameObject building)
        {
            OccupyingBuilding = building;
            IsBuildable = false;
        }

        /// <summary>
        /// Mark this tile as occupied by a building with ID.
        /// </summary>
        public void SetOccupyingBuilding(GameObject building, int buildingId)
        {
            OccupyingBuilding = building;
            OccupyingBuildingId = buildingId;
            IsBuildable = false;
        }

        /// <summary>
        /// Clear the occupying building.
        /// </summary>
        public void ClearOccupyingBuilding()
        {
            OccupyingBuilding = null;
            OccupyingBuildingId = -1;

            // Restore buildability based on terrain config
            if (TerrainConfig != null)
                IsBuildable = TerrainConfig.isBuildable;
            else
                IsBuildable = true;
        }

        /// <summary>
        /// Check if this tile is occupied.
        /// </summary>
        public bool IsOccupied => OccupyingBuilding != null;

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get the world position of this tile.
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            return WorldPosition;
        }

        /// <summary>
        /// Get the biome based on terrain type (fallback when no config).
        /// </summary>
        public BiomeType GetDefaultBiome()
        {
            return TerrainType switch
            {
                TerrainType.Grassland => BiomeType.Temperate,
                TerrainType.Plains => BiomeType.Temperate,
                TerrainType.Forest => BiomeType.Temperate,
                TerrainType.Desert => BiomeType.Arid,
                TerrainType.Tundra => BiomeType.Arctic,
                TerrainType.Snow => BiomeType.Arctic,
                TerrainType.Hills => BiomeType.Mountainous,
                TerrainType.Mountains => BiomeType.Mountainous,
                TerrainType.Water => BiomeType.Aquatic,
                TerrainType.DeepWater => BiomeType.Aquatic,
                TerrainType.Swamp => BiomeType.Aquatic,
                _ => BiomeType.Temperate
            };
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Convert this tile to serializable data.
        /// </summary>
        public HexTileData ToData()
        {
            return HexTileData.FromHexTile(this);
        }

        /// <summary>
        /// Apply data from serialized state.
        /// </summary>
        /// <param name="data">Serialized tile data</param>
        /// <param name="config">Optional terrain config to apply</param>
        public void ApplyData(HexTileData data, TerrainTypeDataSO config = null)
        {
            if (data == null) return;

            TerrainType = data.terrainType;
            OwnerId = data.ownerId;
            OccupyingBuildingId = data.occupyingBuildingId;

            // Load resources
            LoadResources(data.resources);

            // Load per-player visibility states
            LoadVisibility(data.playerVisibility);

            // Apply config if provided
            if (config != null)
            {
                SetTerrainConfig(config);
            }

            // Update buildability based on occupancy
            if (OccupyingBuildingId >= 0)
            {
                IsBuildable = false;
            }
        }

        #endregion
    }
}
