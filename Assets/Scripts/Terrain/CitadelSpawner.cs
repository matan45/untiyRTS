using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTS.Terrain.Core;

namespace RTS.Terrain
{
    /// <summary>
    /// Handles automatic spawning of the Steampunk Citadel on a random buildable tile
    /// after terrain generation is complete.
    /// </summary>
    public class CitadelSpawner : MonoBehaviour
    {
        [Header("Citadel Settings")]
        [Tooltip("Reference to the Steampunk Citadel prefab")]
        [SerializeField] private GameObject citadelPrefab;

        [Tooltip("Wait time in seconds before attempting to spawn citadel")]
        [SerializeField] private float spawnDelay = 0.5f;

        [Tooltip("Automatically spawn citadel on Start")]
        [SerializeField] private bool spawnOnStart = true;

        [Tooltip("Height offset above the tile surface")]
        [SerializeField] private float heightOffset = 0.5f;

        [Header("Spawn Rules")]
        [Tooltip("Valid terrain types for citadel spawning")]
        [SerializeField] private TerrainType[] validTerrainTypes =
        {
            TerrainType.Grassland,
            TerrainType.Plains
        };

        private void Start()
        {
            if (spawnOnStart)
            {
                StartCoroutine(SpawnCitadelAfterTerrainGeneration());
            }
        }

        /// <summary>
        /// Waits for terrain generation to complete, then spawns the citadel.
        /// </summary>
        private IEnumerator SpawnCitadelAfterTerrainGeneration()
        {
            // Wait for terrain to be generated
            yield return new WaitForSeconds(spawnDelay);

            SpawnCitadelOnRandomTile();
        }

        /// <summary>
        /// Spawns the citadel on a random valid buildable tile.
        /// </summary>
        public void SpawnCitadelOnRandomTile()
        {
            if (citadelPrefab == null)
            {
                Debug.LogError("CitadelSpawner: Citadel prefab is not assigned!");
                return;
            }

            HexGridManager gridManager = HexGridManager.Instance;
            if (gridManager == null || gridManager.Grid == null)
            {
                Debug.LogError("CitadelSpawner: HexGridManager not found or Grid is null!");
                return;
            }

            // Get all tiles from the grid
            var allTiles = gridManager.Grid.GetAllTiles();
            if (allTiles == null)
            {
                Debug.LogError("CitadelSpawner: No tiles found in grid!");
                return;
            }

            // Filter for valid buildable tiles
            List<HexTile> validTiles = allTiles
                .Where(tile => tile.IsBuildable &&
                              !tile.IsOccupied &&
                              validTerrainTypes.Contains(tile.TerrainType))
                .ToList();

            if (validTiles.Count == 0)
            {
                Debug.LogWarning("CitadelSpawner: No valid tiles found for citadel spawning!");
                return;
            }

            // Select a random valid tile
            HexTile selectedTile = validTiles[Random.Range(0, validTiles.Count)];

            // Calculate spawn position with height offset
            Vector3 spawnPosition = selectedTile.WorldPosition + Vector3.up * heightOffset;

            // Instantiate the citadel at the tile's position
            // Use prefab's original rotation instead of overriding it
            GameObject citadel = Instantiate(
                citadelPrefab,
                spawnPosition,
                citadelPrefab.transform.rotation
            );

            citadel.name = "Steampunk_Citadel";

            // Register the citadel with the tile
            selectedTile.SetOccupyingBuilding(citadel);

            Debug.Log($"CitadelSpawner: Successfully spawned citadel at coordinates {selectedTile.Coordinates} " +
                     $"(Terrain: {selectedTile.TerrainType})");
        }

        /// <summary>
        /// Manually trigger citadel spawning (useful for testing or runtime spawning).
        /// </summary>
        [ContextMenu("Spawn Citadel Now")]
        public void SpawnCitadelManually()
        {
            SpawnCitadelOnRandomTile();
        }
    }
}
