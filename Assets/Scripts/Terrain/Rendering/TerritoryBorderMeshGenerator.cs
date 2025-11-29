using System.Collections.Generic;
using UnityEngine;
using RTS.Terrain.Core;

namespace RTS.Terrain.Rendering
{
    /// <summary>
    /// Static utility class for generating hex border meshes.
    /// Creates partial border meshes based on edge masks for territory boundaries.
    /// </summary>
    public static class TerritoryBorderMeshGenerator
    {
        // Cache for partial border meshes by edge mask (64 possible combinations for 6 edges)
        private static readonly Dictionary<int, Mesh> _meshCache = new Dictionary<int, Mesh>();

        /// <summary>
        /// Create a full hex border ring mesh.
        /// </summary>
        /// <param name="outerRadius">Outer radius of the hex</param>
        /// <param name="borderWidth">Width of the border ring</param>
        /// <param name="segments">Number of vertices per hex edge (6 = flat)</param>
        /// <returns>Border ring mesh</returns>
        public static Mesh CreateFullBorderRing(float outerRadius, float borderWidth, int segments = 6)
        {
            return CreatePartialBorderMesh(outerRadius, borderWidth, 0b111111, segments);
        }

        /// <summary>
        /// Create a partial hex border mesh based on edge mask.
        /// Uses caching for performance.
        /// </summary>
        /// <param name="outerRadius">Outer radius of the hex</param>
        /// <param name="borderWidth">Width of the border</param>
        /// <param name="edgeMask">6-bit mask indicating which edges to render (bit 0 = East, clockwise)</param>
        /// <param name="segments">Number of vertices per hex edge</param>
        /// <returns>Partial border mesh</returns>
        public static Mesh GetOrCreatePartialBorder(float outerRadius, float borderWidth, int edgeMask, int segments = 6)
        {
            // Clamp to valid edge mask (6 bits)
            edgeMask &= 0b111111;

            // Create cache key including all parameters
            int cacheKey = edgeMask | (segments << 6) | ((int)(outerRadius * 100) << 12) | ((int)(borderWidth * 100) << 22);

            if (_meshCache.TryGetValue(cacheKey, out Mesh cachedMesh))
            {
                return cachedMesh;
            }

            Mesh mesh = CreatePartialBorderMesh(outerRadius, borderWidth, edgeMask, segments);
            _meshCache[cacheKey] = mesh;
            return mesh;
        }

        /// <summary>
        /// Create a partial hex border mesh based on edge mask.
        /// </summary>
        private static Mesh CreatePartialBorderMesh(float outerRadius, float borderWidth, int edgeMask, int segments)
        {
            if (edgeMask == 0)
            {
                // No edges to render
                return new Mesh { name = "EmptyBorder" };
            }

            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            float innerRadius = outerRadius - borderWidth;
            segments = Mathf.Max(6, segments);

            // Flat-top hex: first vertex points right (East)
            // Angle offset for flat-top orientation
            float angleOffset = Mathf.PI / 6f; // 30 degrees

            // Generate vertices for each edge
            for (int edge = 0; edge < 6; edge++)
            {
                // Check if this edge should be rendered
                if ((edgeMask & (1 << edge)) == 0)
                    continue;

                // Calculate angles for this edge's two corners
                float startAngle = (edge * Mathf.PI * 2f / 6f) + angleOffset;
                float endAngle = ((edge + 1) * Mathf.PI * 2f / 6f) + angleOffset;

                int baseIndex = vertices.Count;

                // Outer vertices (2 corners of this edge)
                Vector3 outerStart = new Vector3(
                    Mathf.Cos(startAngle) * outerRadius,
                    0,
                    Mathf.Sin(startAngle) * outerRadius
                );
                Vector3 outerEnd = new Vector3(
                    Mathf.Cos(endAngle) * outerRadius,
                    0,
                    Mathf.Sin(endAngle) * outerRadius
                );

                // Inner vertices
                Vector3 innerStart = new Vector3(
                    Mathf.Cos(startAngle) * innerRadius,
                    0,
                    Mathf.Sin(startAngle) * innerRadius
                );
                Vector3 innerEnd = new Vector3(
                    Mathf.Cos(endAngle) * innerRadius,
                    0,
                    Mathf.Sin(endAngle) * innerRadius
                );

                // Add vertices: outer start, outer end, inner end, inner start
                vertices.Add(outerStart);
                vertices.Add(outerEnd);
                vertices.Add(innerEnd);
                vertices.Add(innerStart);

                // UVs
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 0));

                // Normals (all pointing up)
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                // Triangles (two triangles forming a quad)
                // First triangle: outer start, outer end, inner end
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);

                // Second triangle: outer start, inner end, inner start
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();
            mesh.name = $"TerritoryBorder_{System.Convert.ToString(edgeMask, 2).PadLeft(6, '0')}";

            return mesh;
        }

        /// <summary>
        /// Clear the mesh cache. Call this when changing visual settings.
        /// </summary>
        public static void ClearCache()
        {
            foreach (var mesh in _meshCache.Values)
            {
                if (mesh != null)
                {
                    Object.Destroy(mesh);
                }
            }
            _meshCache.Clear();
        }

        /// <summary>
        /// Calculate edge mask for a tile based on neighbor ownership.
        /// An edge is included if the neighbor is null, neutral, or owned by a different player.
        /// </summary>
        /// <param name="tile">The tile to calculate edge mask for</param>
        /// <param name="grid">The hex grid</param>
        /// <returns>6-bit edge mask</returns>
        public static int CalculateEdgeMask(HexTile tile, HexGrid grid)
        {
            if (tile == null || !tile.HasOwner)
                return 0;

            int mask = 0;
            int ownerId = tile.OwnerId;

            for (int direction = 0; direction < 6; direction++)
            {
                Vector2Int neighborCoord = HexCoordinates.GetNeighborCoordinate(tile.Coordinates, direction);
                HexTile neighbor = grid.GetTile(neighborCoord);

                // Show border on this edge if:
                // - neighbor is null (grid edge)
                // - neighbor is neutral (OwnerId < 0)
                // - neighbor is owned by different player
                if (neighbor == null || neighbor.OwnerId != ownerId)
                {
                    mask |= (1 << direction);
                }
            }

            return mask;
        }
    }
}
