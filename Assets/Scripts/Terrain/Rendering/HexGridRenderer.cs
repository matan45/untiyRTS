using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Terrain.Core;
using RTS.Terrain.Data;

namespace RTS.Terrain.Rendering
{
    /// <summary>
    /// Renders hex grid tiles as 3D mesh objects.
    /// Creates procedural hex cylinder meshes with terrain-based materials and heights.
    /// </summary>
    public class HexGridRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Visual configuration for hex tiles")]
        private HexTileVisualConfigSO visualConfig;

        [SerializeField, Tooltip("Terrain registry for materials and heights")]
        private TerrainTypeRegistry terrainRegistry;

        [SerializeField, Tooltip("Default material when no terrain material is configured")]
        private Material defaultMaterial;

        [Header("Rendering Settings")]
        [SerializeField, Tooltip("Parent transform for tile objects")]
        private Transform tileContainer;

        [SerializeField, Tooltip("Tiles to render per frame (0 = all at once)")]
        [Range(0, 500)]
        private int tilesPerFrame = 100;

        [SerializeField, Tooltip("Enable GPU instancing for materials")]
        private bool useGPUInstancing = true;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo;

        private Dictionary<Vector2Int, HexTileObject> _tileObjects = new Dictionary<Vector2Int, HexTileObject>();
        private Mesh _hexMesh;
        private bool _isRendering;
        private int _hexTileLayer;

        // Fallback colors when no material is configured
        private static readonly Dictionary<TerrainType, Color> FallbackColors = new Dictionary<TerrainType, Color>
        {
            { TerrainType.Grassland, new Color(0.2f, 0.6f, 0.2f) },
            { TerrainType.Plains, new Color(0.7f, 0.6f, 0.3f) },
            { TerrainType.Desert, new Color(0.9f, 0.8f, 0.5f) },
            { TerrainType.Water, new Color(0.2f, 0.4f, 0.8f) },
            { TerrainType.DeepWater, new Color(0.1f, 0.2f, 0.5f) },
            { TerrainType.Forest, new Color(0.1f, 0.4f, 0.1f) },
            { TerrainType.Hills, new Color(0.5f, 0.4f, 0.3f) },
            { TerrainType.Mountains, new Color(0.4f, 0.4f, 0.4f) },
            { TerrainType.Swamp, new Color(0.3f, 0.35f, 0.2f) },
            { TerrainType.Tundra, new Color(0.6f, 0.7f, 0.7f) },
            { TerrainType.Snow, new Color(0.9f, 0.95f, 1f) }
        };

        // Fallback heights when no terrain config specifies height
        private static readonly Dictionary<TerrainType, float> FallbackHeights = new Dictionary<TerrainType, float>
        {
            { TerrainType.DeepWater, -0.5f },
            { TerrainType.Water, -0.3f },
            { TerrainType.Swamp, -0.1f },
            { TerrainType.Grassland, 0f },
            { TerrainType.Forest, 0f },
            { TerrainType.Tundra, 0f },
            { TerrainType.Desert, 0.05f },
            { TerrainType.Plains, 0.1f },
            { TerrainType.Snow, 0.1f },
            { TerrainType.Hills, 0.5f },
            { TerrainType.Mountains, 1.2f }
        };

        private void Awake()
        {
            _hexTileLayer = LayerMask.NameToLayer("HexTile");
            if (_hexTileLayer < 0)
            {
                Debug.LogWarning("HexGridRenderer: 'HexTile' layer not found! Tile selection will not work. Please add 'HexTile' layer in Edit > Project Settings > Tags and Layers");
                _hexTileLayer = 0; // Default layer as fallback
            }

            if (tileContainer == null)
            {
                var containerObj = new GameObject("HexTileContainer");
                containerObj.transform.SetParent(transform);
                tileContainer = containerObj.transform;
            }

            CreateHexMesh();
        }

        /// <summary>
        /// Create the procedural hex mesh used for all tiles.
        /// </summary>
        private void CreateHexMesh()
        {
            int vertexCount = visualConfig != null ? visualConfig.hexVertexCount : 6;
            float height = visualConfig != null ? visualConfig.defaultTileHeight : 0.3f;
            float size = HexCoordinates.HexSize;

            _hexMesh = HexMeshGenerator.CreateFlatTopHexCylinder(size, height, vertexCount);
            _hexMesh.name = "HexTileMesh";
        }

        /// <summary>
        /// Render all tiles in the grid.
        /// </summary>
        /// <param name="grid">The hex grid to render</param>
        public void RenderGrid(HexGrid grid)
        {
            if (grid == null)
            {
                Debug.LogError("HexGridRenderer: Cannot render null grid");
                return;
            }

            if (_isRendering)
            {
                Debug.LogWarning("HexGridRenderer: Already rendering grid");
                return;
            }

            ClearRenderedTiles();

            // Count tiles to render
            int tileCount = 0;
            foreach (var _ in grid.GetAllTiles()) tileCount++;
            Debug.Log($"HexGridRenderer: Starting to render {tileCount} tiles (tilesPerFrame={tilesPerFrame})");

            if (tilesPerFrame <= 0)
            {
                RenderAllTilesImmediate(grid);
            }
            else
            {
                StartCoroutine(RenderGridCoroutine(grid));
            }
        }

        /// <summary>
        /// Render all tiles immediately (may cause frame hitch on large grids).
        /// </summary>
        private void RenderAllTilesImmediate(HexGrid grid)
        {
            foreach (var tile in grid.GetAllTiles())
            {
                CreateTileObject(tile);
            }

            if (showDebugInfo)
            {
                Debug.Log($"HexGridRenderer: Rendered {_tileObjects.Count} tiles immediately");
            }
        }

        /// <summary>
        /// Render tiles over multiple frames to prevent hitching.
        /// </summary>
        private IEnumerator RenderGridCoroutine(HexGrid grid)
        {
            _isRendering = true;
            int count = 0;

            foreach (var tile in grid.GetAllTiles())
            {
                CreateTileObject(tile);
                count++;

                if (count >= tilesPerFrame)
                {
                    count = 0;
                    yield return null;
                }
            }

            _isRendering = false;

            if (showDebugInfo)
            {
                Debug.Log($"HexGridRenderer: Rendered {_tileObjects.Count} tiles");
            }
        }

        /// <summary>
        /// Create a single tile GameObject.
        /// </summary>
        private HexTileObject CreateTileObject(HexTile tile)
        {
            var go = new GameObject($"HexTile_{tile.Coordinates.x}_{tile.Coordinates.y}");
            go.transform.SetParent(tileContainer);
            go.layer = _hexTileLayer;

            // Add collider for raycasting
            var collider = go.AddComponent<MeshCollider>();

            // Add mesh components
            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();

            // Add tile object component
            var tileObj = go.AddComponent<HexTileObject>();
            tileObj.Initialize(tile);

            // Set mesh
            meshFilter.sharedMesh = _hexMesh;
            collider.sharedMesh = _hexMesh;

            // Position the tile
            float terrainHeight = GetTerrainHeight(tile.TerrainType);
            Vector3 position = tile.WorldPosition;
            position.y = terrainHeight;
            go.transform.position = position;

            // Apply material
            Material material = GetTerrainMaterial(tile.TerrainType);
            if (material != null)
            {
                meshRenderer.sharedMaterial = material;
                if (useGPUInstancing)
                {
                    meshRenderer.sharedMaterial.enableInstancing = true;
                }
            }
            else
            {
                // Use fallback color
                meshRenderer.sharedMaterial = defaultMaterial;
                if (defaultMaterial != null)
                {
                    tileObj.RefreshVisualColor(GetFallbackColor(tile.TerrainType));
                }
            }

            _tileObjects[tile.Coordinates] = tileObj;
            return tileObj;
        }

        /// <summary>
        /// Get material for terrain type from registry.
        /// </summary>
        private Material GetTerrainMaterial(TerrainType terrainType)
        {
            if (terrainRegistry == null) return null;

            var config = terrainRegistry.GetConfig(terrainType);
            return config?.defaultMaterial;
        }

        /// <summary>
        /// Get terrain height from registry or fallback.
        /// </summary>
        private float GetTerrainHeight(TerrainType terrainType)
        {
            if (terrainRegistry != null)
            {
                var config = terrainRegistry.GetConfig(terrainType);
                if (config != null)
                {
                    return config.baseHeight;
                }
            }

            return FallbackHeights.TryGetValue(terrainType, out float height) ? height : 0f;
        }

        /// <summary>
        /// Get fallback color for terrain type.
        /// </summary>
        private Color GetFallbackColor(TerrainType terrainType)
        {
            return FallbackColors.TryGetValue(terrainType, out Color color) ? color : Color.magenta;
        }

        /// <summary>
        /// Update the visual for a specific tile.
        /// </summary>
        /// <param name="coordinates">The tile coordinates to update</param>
        public void UpdateTileVisual(Vector2Int coordinates)
        {
            if (!_tileObjects.TryGetValue(coordinates, out var tileObj)) return;

            var tile = tileObj.TileData;
            if (tile == null) return;

            // Update height
            float terrainHeight = GetTerrainHeight(tile.TerrainType);
            Vector3 pos = tileObj.transform.position;
            pos.y = terrainHeight;
            tileObj.transform.position = pos;

            // Update material
            Material material = GetTerrainMaterial(tile.TerrainType);
            if (material != null)
            {
                tileObj.RefreshVisual(material);
            }
            else
            {
                tileObj.RefreshVisualColor(GetFallbackColor(tile.TerrainType));
            }
        }

        /// <summary>
        /// Get the tile object at specific coordinates.
        /// </summary>
        public HexTileObject GetTileObject(Vector2Int coordinates)
        {
            return _tileObjects.TryGetValue(coordinates, out var obj) ? obj : null;
        }

        /// <summary>
        /// Get the tile object from a world position.
        /// </summary>
        public HexTileObject GetTileObjectAtWorldPosition(Vector3 worldPos)
        {
            Vector2Int coords = HexCoordinates.WorldToAxial(worldPos);
            return GetTileObject(coords);
        }

        /// <summary>
        /// Clear all rendered tile objects.
        /// </summary>
        public void ClearRenderedTiles()
        {
            if (_isRendering)
            {
                StopAllCoroutines();
                _isRendering = false;
            }

            foreach (var kvp in _tileObjects)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            _tileObjects.Clear();
        }

        /// <summary>
        /// Get the number of rendered tiles.
        /// </summary>
        public int RenderedTileCount => _tileObjects.Count;

        /// <summary>
        /// Check if rendering is in progress.
        /// </summary>
        public bool IsRendering => _isRendering;

        private void OnDestroy()
        {
            if (_hexMesh != null)
            {
                Destroy(_hexMesh);
            }
        }
    }

    /// <summary>
    /// Utility class for generating hex meshes.
    /// </summary>
    public static class HexMeshGenerator
    {
        /// <summary>
        /// Create a flat-top hexagonal cylinder mesh.
        /// </summary>
        /// <param name="radius">Outer radius of the hex</param>
        /// <param name="height">Height of the cylinder</param>
        /// <param name="segments">Number of segments around the circumference</param>
        public static Mesh CreateFlatTopHexCylinder(float radius, float height, int segments = 6)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            // Ensure segments is at least 6 for a hex
            segments = Mathf.Max(6, segments);

            // Top center vertex
            vertices.Add(new Vector3(0, height, 0));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            // Bottom center vertex
            vertices.Add(new Vector3(0, 0, 0));
            normals.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            // Generate vertices around the circumference
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f); // Offset for flat-top
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                // Top ring
                vertices.Add(new Vector3(x, height, z));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f));

                // Bottom ring
                vertices.Add(new Vector3(x, 0, z));
                normals.Add(Vector3.down);
                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f));

                // Side vertices (top and bottom) with side-facing normals
                Vector3 normal = new Vector3(x, 0, z).normalized;
                vertices.Add(new Vector3(x, height, z));
                normals.Add(normal);
                uvs.Add(new Vector2((float)i / segments, 1));

                vertices.Add(new Vector3(x, 0, z));
                normals.Add(normal);
                uvs.Add(new Vector2((float)i / segments, 0));
            }

            // Generate triangles
            int vertsPerSegment = 4;
            int topCenter = 0;
            int bottomCenter = 1;
            int ringStart = 2;

            for (int i = 0; i < segments; i++)
            {
                int current = ringStart + i * vertsPerSegment;
                int next = ringStart + ((i + 1) % (segments + 1)) * vertsPerSegment;

                // Top face triangle
                triangles.Add(topCenter);
                triangles.Add(current); // top ring
                triangles.Add(next);    // next top ring

                // Bottom face triangle (reversed winding)
                triangles.Add(bottomCenter);
                triangles.Add(next + 1);    // next bottom ring
                triangles.Add(current + 1); // bottom ring

                // Side quad (two triangles)
                int sideTop = current + 2;
                int sideBottom = current + 3;
                int nextSideTop = next + 2;
                int nextSideBottom = next + 3;

                // Triangle 1
                triangles.Add(sideTop);
                triangles.Add(nextSideTop);
                triangles.Add(sideBottom);

                // Triangle 2
                triangles.Add(sideBottom);
                triangles.Add(nextSideTop);
                triangles.Add(nextSideBottom);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }
    }
}
