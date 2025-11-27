using UnityEngine;

namespace RTS.Terrain.Core
{
    /// <summary>
    /// Utility class for hexagonal coordinate conversions.
    /// Uses flat-top hexagons with axial coordinate system.
    /// </summary>
    public static class HexCoordinates
    {
        private const float SQRT_3 = 1.732050808f;
        public static float HexSize = 1f;

        /// <summary>
        /// Direction offsets for 6 neighbors of a flat-top hex (axial coordinates).
        /// Order: East, Northeast, Northwest, West, Southwest, Southeast (clockwise from East)
        /// </summary>
        public static readonly Vector2Int[] NeighborDirections = new Vector2Int[]
        {
            new Vector2Int(+1,  0), // East
            new Vector2Int(+1, -1), // Northeast
            new Vector2Int( 0, -1), // Northwest
            new Vector2Int(-1,  0), // West
            new Vector2Int(-1, +1), // Southwest
            new Vector2Int( 0, +1)  // Southeast
        };

        /// <summary>
        /// Get coordinates of all 6 neighboring hexes.
        /// Does not validate if coordinates exist in any grid.
        /// </summary>
        /// <param name="center">The center hex coordinate</param>
        /// <returns>Array of 6 neighbor coordinates</returns>
        public static Vector2Int[] GetNeighborCoordinates(Vector2Int center)
        {
            Vector2Int[] neighbors = new Vector2Int[6];
            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = center + NeighborDirections[i];
            }
            return neighbors;
        }

        /// <summary>
        /// Get a specific neighbor by direction index (0-5, clockwise from East).
        /// </summary>
        /// <param name="center">The center hex coordinate</param>
        /// <param name="direction">Direction index (0=East, 1=NE, 2=NW, 3=West, 4=SW, 5=SE)</param>
        /// <returns>The neighbor coordinate in the specified direction</returns>
        public static Vector2Int GetNeighborCoordinate(Vector2Int center, int direction)
        {
            return center + NeighborDirections[direction % 6];
        }

        /// <summary>
        /// Convert axial coordinates (q, r) to world position.
        /// </summary>
        public static Vector3 AxialToWorld(Vector2Int axial)
        {
            return AxialToWorld(axial.x, axial.y);
        }

        /// <summary>
        /// Convert axial coordinates (q, r) to world position.
        /// </summary>
        public static Vector3 AxialToWorld(int q, int r)
        {
            float x = HexSize * (SQRT_3 * q + SQRT_3 / 2f * r);
            float z = HexSize * (3f / 2f * r);
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// Convert world position to axial coordinates (q, r).
        /// </summary>
        public static Vector2Int WorldToAxial(Vector3 worldPos)
        {
            float q = (SQRT_3 / 3f * worldPos.x - 1f / 3f * worldPos.z) / HexSize;
            float r = (2f / 3f * worldPos.z) / HexSize;

            return CubeToAxial(CubeRound(AxialToCube(q, r)));
        }

        /// <summary>
        /// Convert axial to cube coordinates for rounding.
        /// </summary>
        private static Vector3 AxialToCube(float q, float r)
        {
            float x = q;
            float z = r;
            float y = -x - z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Round fractional cube coordinates to nearest hex.
        /// </summary>
        private static Vector3 CubeRound(Vector3 cube)
        {
            float rx = Mathf.Round(cube.x);
            float ry = Mathf.Round(cube.y);
            float rz = Mathf.Round(cube.z);

            float xDiff = Mathf.Abs(rx - cube.x);
            float yDiff = Mathf.Abs(ry - cube.y);
            float zDiff = Mathf.Abs(rz - cube.z);

            if (xDiff > yDiff && xDiff > zDiff)
                rx = -ry - rz;
            else if (yDiff > zDiff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new Vector3(rx, ry, rz);
        }

        /// <summary>
        /// Convert cube coordinates back to axial.
        /// </summary>
        private static Vector2Int CubeToAxial(Vector3 cube)
        {
            int q = Mathf.RoundToInt(cube.x);
            int r = Mathf.RoundToInt(cube.z);
            return new Vector2Int(q, r);
        }

        /// <summary>
        /// Calculate distance between two hex coordinates.
        /// </summary>
        public static int Distance(Vector2Int a, Vector2Int b)
        {
            Vector3 cubeA = AxialToCube(a.x, a.y);
            Vector3 cubeB = AxialToCube(b.x, b.y);

            return Mathf.RoundToInt(
                (Mathf.Abs(cubeA.x - cubeB.x) +
                 Mathf.Abs(cubeA.y - cubeB.y) +
                 Mathf.Abs(cubeA.z - cubeB.z)) / 2f
            );
        }
    }
}
