using UnityEngine;
using RTS.Terrain.Core;

namespace RTS.Terrain
{
    /// <summary>
    /// Generates 3D hexagonal terrain with elevation.
    /// Creates actual 3D GameObjects for each hex tile with height variations.
    /// </summary>
    public class Hex3DTerrainGenerator : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private int mapWidth = 20;
        [SerializeField] private int mapHeight = 20;
        [SerializeField] private float hexSize = 1f;

        [Header("Terrain Heights")]
        [SerializeField] private float waterHeight = -0.5f;
        [SerializeField] private float grassHeight = 0f;
        [SerializeField] private float plainsHeight = 0.1f;
        [SerializeField] private float hillsHeight = 0.5f;
        [SerializeField] private float mountainHeight = 1.2f;

        [Header("Materials")]
        [SerializeField, Tooltip("Material for water terrain. If not assigned, will be created at runtime.")]
        private Material waterMaterial;
        [SerializeField, Tooltip("Material for grass terrain. If not assigned, will be created at runtime.")]
        private Material grassMaterial;
        [SerializeField, Tooltip("Material for plains terrain. If not assigned, will be created at runtime.")]
        private Material plainsMaterial;
        [SerializeField, Tooltip("Material for hills terrain. If not assigned, will be created at runtime.")]
        private Material hillsMaterial;
        [SerializeField, Tooltip("Material for mountain terrain. If not assigned, will be created at runtime.")]
        private Material mountainMaterial;

        [Header("Generation")]
        [SerializeField] private float noiseScale = 0.1f;
        [SerializeField] private bool generateOnStart = true;

        private GameObject terrainContainer;

        // Runtime material tracking (for cleanup)
        private bool waterMaterialCreatedAtRuntime = false;
        private bool grassMaterialCreatedAtRuntime = false;
        private bool plainsMaterialCreatedAtRuntime = false;
        private bool hillsMaterialCreatedAtRuntime = false;
        private bool mountainMaterialCreatedAtRuntime = false;

        // Hex constants for flat-top hexagons
        private const float SQRT_3 = 1.732050808f;

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateTerrain();
            }
        }

        [ContextMenu("Generate 3D Terrain")]
        public void GenerateTerrain()
        {
            // Clear existing terrain
            if (terrainContainer != null)
            {
                DestroyImmediate(terrainContainer);
            }

            terrainContainer = new GameObject("3D Terrain");
            terrainContainer.transform.parent = transform;
            terrainContainer.transform.localPosition = Vector3.zero;

            // Create materials if not assigned
            CreateDefaultMaterialsIfNeeded();

            // Generate hex tiles
            for (int q = 0; q < mapWidth; q++)
            {
                for (int r = 0; r < mapHeight; r++)
                {
                    CreateHexTile(q, r);
                }
            }

            Debug.Log($"Generated {mapWidth}x{mapHeight} 3D hex terrain!");
        }

        private void CreateHexTile(int q, int r)
        {
            // Calculate world position for this hex
            Vector3 worldPos = AxialToWorld(q, r);

            // Get terrain type based on noise
            float noise = Mathf.PerlinNoise(q * noiseScale, r * noiseScale);
            TerrainInfo terrainInfo = GetTerrainFromNoise(noise);

            // Create hex tile GameObject
            GameObject hexTile = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hexTile.name = $"Hex_{q}_{r}";
            hexTile.transform.parent = terrainContainer.transform;

            // Position and scale
            float height = terrainInfo.height;
            hexTile.transform.position = new Vector3(worldPos.x, height / 2f, worldPos.z);

            // Scale: make it look like a hex (flat cylinder)
            float hexWidth = hexSize * SQRT_3;
            hexTile.transform.localScale = new Vector3(hexWidth * 0.95f, height + 0.1f, hexWidth * 0.95f);

            // Apply material
            MeshRenderer renderer = hexTile.GetComponent<MeshRenderer>();
            if (renderer != null && terrainInfo.material != null)
            {
                renderer.material = terrainInfo.material;
            }

            // Optional: Add collider for clicking/selection
            // The primitive already has a collider

            // Register with HexGridManager
            HexGridManager gridManager = HexGridManager.Instance;
            if (gridManager != null)
            {
                gridManager.RegisterTerrainTile(q, r, hexTile, terrainInfo.terrainType);
            }
        }

        private Vector3 AxialToWorld(int q, int r)
        {
            float x = hexSize * (SQRT_3 * q + SQRT_3 / 2f * r);
            float z = hexSize * (3f / 2f * r);
            return new Vector3(x, 0, z);
        }

        private TerrainInfo GetTerrainFromNoise(float noise)
        {
            if (noise < 0.25f)
                return new TerrainInfo { height = waterHeight, material = waterMaterial, terrainType = TerrainType.Water };
            else if (noise < 0.45f)
                return new TerrainInfo { height = grassHeight, material = grassMaterial, terrainType = TerrainType.Grassland };
            else if (noise < 0.6f)
                return new TerrainInfo { height = plainsHeight, material = plainsMaterial, terrainType = TerrainType.Plains };
            else if (noise < 0.8f)
                return new TerrainInfo { height = hillsHeight, material = hillsMaterial, terrainType = TerrainType.Hills };
            else
                return new TerrainInfo { height = mountainHeight, material = mountainMaterial, terrainType = TerrainType.Mountains };
        }

        private void CreateDefaultMaterialsIfNeeded()
        {
            // Create water material if not assigned
            if (waterMaterial == null)
            {
                waterMaterial = new Material(Shader.Find("Standard"));
                waterMaterial.color = new Color(0.2f, 0.4f, 0.8f); // Blue
                waterMaterialCreatedAtRuntime = true;
            }

            // Create grass material if not assigned
            if (grassMaterial == null)
            {
                grassMaterial = new Material(Shader.Find("Standard"));
                grassMaterial.color = new Color(0.2f, 0.6f, 0.2f); // Green
                grassMaterialCreatedAtRuntime = true;
            }

            // Create plains material if not assigned
            if (plainsMaterial == null)
            {
                plainsMaterial = new Material(Shader.Find("Standard"));
                plainsMaterial.color = new Color(0.7f, 0.6f, 0.3f); // Tan
                plainsMaterialCreatedAtRuntime = true;
            }

            // Create hills material if not assigned
            if (hillsMaterial == null)
            {
                hillsMaterial = new Material(Shader.Find("Standard"));
                hillsMaterial.color = new Color(0.5f, 0.4f, 0.3f); // Brown
                hillsMaterialCreatedAtRuntime = true;
            }

            // Create mountain material if not assigned
            if (mountainMaterial == null)
            {
                mountainMaterial = new Material(Shader.Find("Standard"));
                mountainMaterial.color = new Color(0.4f, 0.4f, 0.4f); // Gray
                mountainMaterialCreatedAtRuntime = true;
            }
        }

        [ContextMenu("Clear Terrain")]
        public void ClearTerrain()
        {
            if (terrainContainer != null)
            {
                DestroyImmediate(terrainContainer);
            }
        }

        private void OnDestroy()
        {
            // Destroy runtime-created materials to prevent memory leaks
            if (waterMaterialCreatedAtRuntime && waterMaterial != null)
            {
                Destroy(waterMaterial);
                waterMaterial = null;
            }

            if (grassMaterialCreatedAtRuntime && grassMaterial != null)
            {
                Destroy(grassMaterial);
                grassMaterial = null;
            }

            if (plainsMaterialCreatedAtRuntime && plainsMaterial != null)
            {
                Destroy(plainsMaterial);
                plainsMaterial = null;
            }

            if (hillsMaterialCreatedAtRuntime && hillsMaterial != null)
            {
                Destroy(hillsMaterial);
                hillsMaterial = null;
            }

            if (mountainMaterialCreatedAtRuntime && mountainMaterial != null)
            {
                Destroy(mountainMaterial);
                mountainMaterial = null;
            }
        }

        private struct TerrainInfo
        {
            public float height;
            public Material material;
            public TerrainType terrainType;
        }
    }
}
