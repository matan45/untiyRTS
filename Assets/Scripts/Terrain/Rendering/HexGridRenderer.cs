using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RTS.Terrain.Core;
using RTS.Terrain.Data;

namespace RTS.Terrain.Rendering
{
    /// <summary>
    /// Renders hex grid tiles using GPU instancing for performance.
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

        [SerializeField, Tooltip("Use GPU instancing for rendering (major performance improvement)")]
        private bool useGPUInstancing = true;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo;

        private Dictionary<Vector2Int, HexTileObject> _tileObjects = new Dictionary<Vector2Int, HexTileObject>();
        private Mesh _hexMesh;
        private bool _isRendering;
        private int _hexTileLayer;

        // GPU Instancing data
        private Dictionary<Material, List<Matrix4x4>> _instanceMatrices = new Dictionary<Material, List<Matrix4x4>>();

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
            float bevelSize = visualConfig != null ? visualConfig.bevelSize : 0.05f;
            float borderWidth = visualConfig != null ? visualConfig.borderWidth : 0.08f;
            float centerDepth = visualConfig != null ? visualConfig.centerDepth : 0.02f;
            float size = HexCoordinates.HexSize;

            _hexMesh = HexMeshGenerator.CreateFlatTopHexCylinder(
                size,
                height,
                vertexCount,
                bevelSize,
                borderWidth,
                centerDepth
            );
            _hexMesh.name = "HexTileMesh";
        }

        private void LateUpdate()
        {
            if (useGPUInstancing && _instanceMatrices.Count > 0)
            {
                RenderInstancedTiles();
            }
        }

        /// <summary>
        /// Render all tiles using GPU instancing.
        /// </summary>
        private void RenderInstancedTiles()
        {
            foreach (var kvp in _instanceMatrices)
            {
                Material material = kvp.Key;
                List<Matrix4x4> matrices = kvp.Value;

                if (matrices.Count == 0) continue;
                if (material == null || !material.enableInstancing) continue;

                var rp = new RenderParams(material)
                {
                    layer = _hexTileLayer,
                    shadowCastingMode = ShadowCastingMode.On,
                    receiveShadows = true
                };

                // RenderMeshInstanced supports up to 1023 instances per call
                int batchSize = 1023;
                for (int i = 0; i < matrices.Count; i += batchSize)
                {
                    int count = Mathf.Min(batchSize, matrices.Count - i);
                    Graphics.RenderMeshInstanced(rp, _hexMesh, 0, matrices, count, i);
                }
            }
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
            collider.sharedMesh = _hexMesh;

            // Position the tile
            float terrainHeight = GetTerrainHeight(tile.TerrainType);
            Vector3 position = tile.WorldPosition;
            position.y = terrainHeight;
            go.transform.position = position;

            // Get material for this terrain type
            Material material = GetTerrainMaterial(tile.TerrainType) ?? defaultMaterial;

            // Check if we can use GPU instancing (material must support it)
            bool canUseInstancing = useGPUInstancing && material != null && material.enableInstancing;

            if (canUseInstancing)
            {
                // GPU Instancing mode: store transform matrix for batched rendering
                AddToInstanceBatch(material, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));

                // Add lightweight tile object (no MeshRenderer needed)
                var tileObj = go.AddComponent<HexTileObject>();
                tileObj.Initialize(tile);
                _tileObjects[tile.Coordinates] = tileObj;
                return tileObj;
            }
            else
            {
                // Traditional mode: individual MeshRenderer per tile
                var meshFilter = go.AddComponent<MeshFilter>();
                var meshRenderer = go.AddComponent<MeshRenderer>();

                var tileObj = go.AddComponent<HexTileObject>();
                tileObj.Initialize(tile);

                meshFilter.sharedMesh = _hexMesh;

                if (material != null)
                {
                    meshRenderer.sharedMaterial = material;
                }
                else if (defaultMaterial != null)
                {
                    meshRenderer.sharedMaterial = defaultMaterial;
                    tileObj.RefreshVisualColor(GetFallbackColor(tile.TerrainType));
                }

                _tileObjects[tile.Coordinates] = tileObj;
                return tileObj;
            }
        }

        /// <summary>
        /// Add a tile transform to the GPU instancing batch for a material.
        /// </summary>
        private void AddToInstanceBatch(Material material, Matrix4x4 matrix)
        {
            if (!_instanceMatrices.TryGetValue(material, out var matrices))
            {
                matrices = new List<Matrix4x4>();
                _instanceMatrices[material] = matrices;
            }
            matrices.Add(matrix);
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

            // Clear GPU instancing data
            foreach (var matrices in _instanceMatrices.Values)
            {
                matrices.Clear();
            }
            _instanceMatrices.Clear();
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
    /// Utility class for generating hex meshes with visual enhancements.
    /// </summary>
    public static class HexMeshGenerator
    {
        /// <summary>
        /// Create a flat-top hexagonal cylinder mesh with beveled edges, border, and top detail.
        /// </summary>
        /// <param name="radius">Outer radius of the hex</param>
        /// <param name="height">Height of the cylinder</param>
        /// <param name="segments">Number of segments (6 for hex, higher for smoother)</param>
        /// <param name="bevelSize">Size of the bevel (0 = no bevel)</param>
        /// <param name="borderWidth">Width of the border inset on top (0 = no border)</param>
        /// <param name="centerDepth">Depth of center depression for top detail (0 = flat)</param>
        public static Mesh CreateFlatTopHexCylinder(
            float radius,
            float height,
            int segments = 6,
            float bevelSize = 0.05f,
            float borderWidth = 0.08f,
            float centerDepth = 0.02f)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();

            segments = Mathf.Max(6, segments);

            float innerRadius = radius - borderWidth;
            float bevelHeight = height - bevelSize;
            float bevelRadius = radius - bevelSize;

            // === TOP SURFACE WITH DETAIL ===

            // Center vertex (slightly depressed for detail)
            int centerIdx = vertices.Count;
            vertices.Add(new Vector3(0, height - centerDepth, 0));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));
            colors.Add(Color.white);

            // Inner ring (border inside edge)
            int innerRingStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * innerRadius;
                float z = Mathf.Sin(angle) * innerRadius;

                vertices.Add(new Vector3(x, height - centerDepth * 0.5f, z));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2((Mathf.Cos(angle) * 0.4f + 0.5f), (Mathf.Sin(angle) * 0.4f + 0.5f)));
                colors.Add(Color.white);
            }

            // Outer top ring (at border)
            int outerTopRingStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * bevelRadius;
                float z = Mathf.Sin(angle) * bevelRadius;

                vertices.Add(new Vector3(x, height, z));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f));
                colors.Add(new Color(0.7f, 0.7f, 0.7f, 1f)); // Darker border
            }

            // === BEVEL EDGE ===

            // Bevel top vertices
            int bevelTopStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * bevelRadius;
                float z = Mathf.Sin(angle) * bevelRadius;

                Vector3 outward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 bevelNormal = (outward + Vector3.up).normalized;

                vertices.Add(new Vector3(x, height, z));
                normals.Add(bevelNormal);
                uvs.Add(new Vector2((float)i / segments, 1f));
                colors.Add(new Color(0.8f, 0.8f, 0.8f, 1f));
            }

            // Bevel bottom vertices (where bevel meets side)
            int bevelBottomStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                Vector3 outward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 bevelNormal = (outward + Vector3.up).normalized;

                vertices.Add(new Vector3(x, bevelHeight, z));
                normals.Add(bevelNormal);
                uvs.Add(new Vector2((float)i / segments, 0.9f));
                colors.Add(new Color(0.9f, 0.9f, 0.9f, 1f));
            }

            // === SIDE WALL ===

            // Side top vertices
            int sideTopStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                Vector3 normal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                vertices.Add(new Vector3(x, bevelHeight, z));
                normals.Add(normal);
                uvs.Add(new Vector2((float)i / segments, 0.85f));
                colors.Add(Color.white);
            }

            // Side bottom vertices
            int sideBottomStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                Vector3 normal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                vertices.Add(new Vector3(x, 0, z));
                normals.Add(normal);
                uvs.Add(new Vector2((float)i / segments, 0f));
                colors.Add(new Color(0.6f, 0.6f, 0.6f, 1f)); // Darker at bottom
            }

            // === BOTTOM FACE ===

            int bottomCenterIdx = vertices.Count;
            vertices.Add(new Vector3(0, 0, 0));
            normals.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));
            colors.Add(Color.white);

            int bottomRingStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                vertices.Add(new Vector3(x, 0, z));
                normals.Add(Vector3.down);
                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f));
                colors.Add(Color.white);
            }

            // === GENERATE TRIANGLES ===

            for (int i = 0; i < segments; i++)
            {
                int next = i + 1;

                // Top center to inner ring
                triangles.Add(centerIdx);
                triangles.Add(innerRingStart + next);
                triangles.Add(innerRingStart + i);

                // Inner ring to outer top ring (border strip)
                triangles.Add(innerRingStart + i);
                triangles.Add(innerRingStart + next);
                triangles.Add(outerTopRingStart + i);

                triangles.Add(outerTopRingStart + i);
                triangles.Add(innerRingStart + next);
                triangles.Add(outerTopRingStart + next);

                // Bevel face
                triangles.Add(bevelTopStart + i);
                triangles.Add(bevelTopStart + next);
                triangles.Add(bevelBottomStart + i);

                triangles.Add(bevelBottomStart + i);
                triangles.Add(bevelTopStart + next);
                triangles.Add(bevelBottomStart + next);

                // Side wall
                triangles.Add(sideTopStart + i);
                triangles.Add(sideTopStart + next);
                triangles.Add(sideBottomStart + i);

                triangles.Add(sideBottomStart + i);
                triangles.Add(sideTopStart + next);
                triangles.Add(sideBottomStart + next);

                // Bottom face
                triangles.Add(bottomCenterIdx);
                triangles.Add(bottomRingStart + i);
                triangles.Add(bottomRingStart + next);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);

            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }
    }
}
