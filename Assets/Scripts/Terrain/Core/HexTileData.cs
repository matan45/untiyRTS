using System;
using System.Linq;
using UnityEngine;
using RTS.Terrain.Data;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Serializable data class for saving/loading hex tile state.
    /// Contains only primitive/serializable types for JsonUtility compatibility.
    /// </summary>
    [Serializable]
    public class HexTileData
    {
        // Coordinates (axial)
        public int q;
        public int r;

        // Terrain
        public TerrainType terrainType;

        // Ownership (-1 = unowned, 0+ = player ID)
        public int ownerId = -1;

        // Resources (serialized as array for JsonUtility)
        public TileResourceValue[] resources;

        // Per-player visibility states for Fog of War
        public PlayerVisibilityState[] playerVisibility;

        // State
        public int occupyingBuildingId = -1;

        /// <summary>
        /// Create an empty HexTileData.
        /// </summary>
        public HexTileData()
        {
            q = 0;
            r = 0;
            terrainType = TerrainType.Grassland;
            ownerId = -1;
            resources = Array.Empty<TileResourceValue>();
            playerVisibility = Array.Empty<PlayerVisibilityState>();
            occupyingBuildingId = -1;
        }

        /// <summary>
        /// Create HexTileData with coordinates and terrain type.
        /// </summary>
        public HexTileData(int q, int r, TerrainType terrain)
        {
            this.q = q;
            this.r = r;
            terrainType = terrain;
            ownerId = -1;
            resources = Array.Empty<TileResourceValue>();
            playerVisibility = Array.Empty<PlayerVisibilityState>();
            occupyingBuildingId = -1;
        }

        /// <summary>
        /// Create HexTileData from an existing HexTile.
        /// </summary>
        public static HexTileData FromHexTile(HexTile tile)
        {
            if (tile == null)
                throw new ArgumentNullException(nameof(tile));

            return new HexTileData
            {
                q = tile.Coordinates.x,
                r = tile.Coordinates.y,
                terrainType = tile.TerrainType,
                ownerId = tile.OwnerId,
                resources = tile.GetResourcesAsArray(),
                playerVisibility = tile.GetVisibilityAsArray(),
                occupyingBuildingId = tile.OccupyingBuildingId
            };
        }

        /// <summary>
        /// Get coordinates as Vector2Int.
        /// </summary>
        public Vector2Int GetCoordinates()
        {
            return new Vector2Int(q, r);
        }

        /// <summary>
        /// Check if this tile has an owner.
        /// </summary>
        public bool HasOwner => ownerId >= 0;

        /// <summary>
        /// Check if this tile is occupied by a building.
        /// </summary>
        public bool IsOccupied => occupyingBuildingId >= 0;
    }

    /// <summary>
    /// Container for complete hex grid save data.
    /// Supports versioning for future migration.
    /// </summary>
    [Serializable]
    public class HexGridSaveData
    {
            
        public int width;
        public int height;
        public string timestamp;
        public HexTileData[] tiles;

        /// <summary>
        /// Create empty save data.
        /// </summary>
        public HexGridSaveData()
        {
            width = 0;
            height = 0;
            timestamp = DateTime.UtcNow.ToString("o");
            tiles = Array.Empty<HexTileData>();
        }

        /// <summary>
        /// Create save data with dimensions.
        /// </summary>
        public HexGridSaveData(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.timestamp = DateTime.UtcNow.ToString("o");
            this.tiles = Array.Empty<HexTileData>();
        }

        /// <summary>
        /// Serialize to JSON string.
        /// </summary>
        public string ToJson(bool prettyPrint = false)
        {
            timestamp = DateTime.UtcNow.ToString("o");
            return JsonUtility.ToJson(this, prettyPrint);
        }

        /// <summary>
        /// Deserialize from JSON string.
        /// </summary>
        public static HexGridSaveData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Cannot parse empty JSON for HexGridSaveData.");
                return null;
            }

            try
            {
              
                return JsonUtility.FromJson<HexGridSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse HexGridSaveData from JSON: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the number of tiles in this save data.
        /// </summary>
        public int TileCount => tiles?.Length ?? 0;

        /// <summary>
        /// Validate save data integrity.
        /// </summary>
        public bool Validate()
        {
            if (tiles == null)
            {
                Debug.LogError("HexGridSaveData has null tiles array.");
                return false;
            }

            if (width <= 0 || height <= 0)
            {
                Debug.LogError($"HexGridSaveData has invalid dimensions: {width}x{height}");
                return false;
            }

            return true;
        }
    }
}
