using System;
using UnityEngine;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// Types of resources that can be found on or produced by tiles.
    /// </summary>
    public enum ResourceType
    {
        Gold,
        Food,
        Production,
        Science,
        // Strategic resources
        Iron,
        Oil,
        Uranium
    }

    /// <summary>
    /// Defines the base resource yield for a terrain type.
    /// Used in TerrainTypeDataSO to configure possible resources.
    /// </summary>
    [Serializable]
    public struct TileResourceYield
    {
        [Tooltip("The type of resource this terrain can yield")]
        public ResourceType resourceType;

        [Tooltip("Base yield amount per turn")]
        [Range(0, 100)]
        public int baseYield;

        [Tooltip("Multiplier for yield calculations (improvements, bonuses)")]
        [Range(0f, 5f)]
        public float yieldMultiplier;

        public TileResourceYield(ResourceType type, int yield, float multiplier = 1f)
        {
            resourceType = type;
            baseYield = yield;
            yieldMultiplier = multiplier;
        }

        /// <summary>
        /// Calculate the effective yield with multiplier applied.
        /// </summary>
        public int GetEffectiveYield()
        {
            return Mathf.RoundToInt(baseYield * yieldMultiplier);
        }
    }

    /// <summary>
    /// Runtime resource value for a specific tile.
    /// Tracks current amount and maximum capacity.
    /// </summary>
    [Serializable]
    public class TileResourceValue
    {
        public ResourceType resourceType;
        public int currentAmount;
        public int maxAmount;

        public TileResourceValue()
        {
            resourceType = ResourceType.Gold;
            currentAmount = 0;
            maxAmount = 100;
        }

        public TileResourceValue(ResourceType type, int current, int max)
        {
            resourceType = type;
            currentAmount = current;
            maxAmount = max;
        }

        /// <summary>
        /// Check if resources can be harvested.
        /// </summary>
        public bool CanHarvest(int amount)
        {
            return currentAmount >= amount;
        }

        /// <summary>
        /// Attempt to harvest resources. Returns true if successful.
        /// </summary>
        public bool Harvest(int amount)
        {
            if (!CanHarvest(amount))
                return false;

            currentAmount -= amount;
            return true;
        }

        /// <summary>
        /// Add resources up to max capacity. Returns amount actually added.
        /// </summary>
        public int Add(int amount)
        {
            int spaceAvailable = maxAmount - currentAmount;
            int toAdd = Mathf.Min(amount, spaceAvailable);
            currentAmount += toAdd;
            return toAdd;
        }

        /// <summary>
        /// Check if this resource is depleted.
        /// </summary>
        public bool IsDepleted => currentAmount <= 0;

        /// <summary>
        /// Check if this resource is at max capacity.
        /// </summary>
        public bool IsFull => currentAmount >= maxAmount;

        /// <summary>
        /// Get fill percentage (0-1).
        /// </summary>
        public float FillPercentage => maxAmount > 0 ? (float)currentAmount / maxAmount : 0f;

        /// <summary>
        /// Create a copy of this resource value.
        /// </summary>
        public TileResourceValue Clone()
        {
            return new TileResourceValue(resourceType, currentAmount, maxAmount);
        }
    }
}
