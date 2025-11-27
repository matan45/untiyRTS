using System.Collections.Generic;
using UnityEngine;
using RTS.Terrain.Core;
using RTS.Terrain.Data;
using RTS.Terrain.Rendering;

namespace RTS.Terrain.Selection
{
    /// <summary>
    /// Visual feedback component for hex tile selection.
    /// Creates overlay meshes for selected and hovered tiles.
    /// Uses object pooling for performance.
    /// </summary>
    public class HexTileSelectionVisual : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("Visual configuration for overlays")]
        private HexTileVisualConfigSO visualConfig;

        [Header("References")]
        [SerializeField, Tooltip("Selection manager to subscribe to")]
        private HexTileSelectionManager selectionManager;

        [Header("Pool Settings")]
        [SerializeField, Tooltip("Initial pool size for overlay objects")]
        private int initialPoolSize = 5;

        private GameObject _selectionOverlay;
        private GameObject _hoverOverlay;
        private Queue<GameObject> _overlayPool = new Queue<GameObject>();
        private Mesh _overlayMesh;
        private Material _selectionMaterial;
        private Material _hoverMaterial;
        private float _pulseTime;

        private void Awake()
        {
            CreateOverlayMesh();
            CreateMaterials();
            InitializePool();
        }

        private void Start()
        {
            // Subscribe in Start to ensure HexTileSelectionManager.Awake() has run
            TrySubscribe();
        }

        private void OnEnable()
        {
            // Re-subscribe when re-enabled (if already initialized)
            if (selectionManager != null)
            {
                Subscribe();
            }
        }

        private void TrySubscribe()
        {
            if (selectionManager == null)
            {
                selectionManager = HexTileSelectionManager.Instance;
            }

            if (selectionManager != null)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            selectionManager.OnTileSelected += OnTileSelected;
            selectionManager.OnTileDeselected += OnTileDeselected;
            selectionManager.OnTileHovered += OnTileHovered;
            selectionManager.OnTileHoverExit += OnTileHoverExit;
        }

        private void OnDisable()
        {
            if (selectionManager != null)
            {
                selectionManager.OnTileSelected -= OnTileSelected;
                selectionManager.OnTileDeselected -= OnTileDeselected;
                selectionManager.OnTileHovered -= OnTileHovered;
                selectionManager.OnTileHoverExit -= OnTileHoverExit;
            }
        }

        private void Update()
        {
            if (_selectionOverlay != null && _selectionOverlay.activeSelf && visualConfig != null && visualConfig.enablePulse)
            {
                _pulseTime += Time.deltaTime;
                UpdatePulseAnimation();
            }
        }

        /// <summary>
        /// Create the overlay mesh (flat hex shape).
        /// </summary>
        private void CreateOverlayMesh()
        {
            int segments = visualConfig != null ? visualConfig.hexVertexCount : 6;
            float radius = HexCoordinates.HexSize;

            _overlayMesh = CreateFlatHexMesh(radius, segments);
            _overlayMesh.name = "HexOverlayMesh";
        }

        /// <summary>
        /// Create a flat hex mesh for overlays.
        /// </summary>
        private Mesh CreateFlatHexMesh(float radius, int segments)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            segments = Mathf.Max(6, segments);

            // Center vertex
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0.5f));
            normals.Add(Vector3.up);

            // Outer vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * Mathf.PI * 2f / segments) + (Mathf.PI / 6f); // Flat-top offset
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                vertices.Add(new Vector3(x, 0, z));
                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f));
                normals.Add(Vector3.up);
            }

            // Triangles - wind counter-clockwise for upward-facing normals
            for (int i = 1; i <= segments; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Create materials for selection and hover overlays.
        /// </summary>
        private void CreateMaterials()
        {
            // Selection material
            if (visualConfig != null && visualConfig.selectionOverlayMaterial != null)
            {
                _selectionMaterial = new Material(visualConfig.selectionOverlayMaterial);
            }
            else
            {
                _selectionMaterial = CreateDefaultOverlayMaterial();
            }

            Color selectionColor = visualConfig != null ? visualConfig.selectionColor : new Color(1f, 1f, 0f, 0.5f);
            _selectionMaterial.color = selectionColor;
            _selectionMaterial.SetColor("_BaseColor", selectionColor);

            // Hover material
            if (visualConfig != null && visualConfig.GetHoverMaterial() != null)
            {
                _hoverMaterial = new Material(visualConfig.GetHoverMaterial());
            }
            else
            {
                _hoverMaterial = CreateDefaultOverlayMaterial();
            }

            Color hoverColor = visualConfig != null ? visualConfig.hoverColor : new Color(1f, 1f, 1f, 0.4f);
            _hoverMaterial.color = hoverColor;
            _hoverMaterial.SetColor("_BaseColor", hoverColor);
        }

        /// <summary>
        /// Create a default transparent overlay material.
        /// </summary>
        private Material CreateDefaultOverlayMaterial()
        {
            // Try to use URP Lit shader for proper transparency support
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);

            // URP transparency setup
            material.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            material.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
            material.SetFloat("_AlphaClip", 0); // Disable alpha clipping
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_Cull", 0); // Cull Off - render both sides

            // Set render queue for transparency
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Enable URP transparency keyword
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            // Disable keywords that might interfere
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            return material;
        }

        /// <summary>
        /// Initialize the object pool for overlays.
        /// </summary>
        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                var overlay = CreateOverlayObject();
                overlay.SetActive(false);
                _overlayPool.Enqueue(overlay);
            }
        }

        /// <summary>
        /// Create a single overlay GameObject.
        /// </summary>
        private GameObject CreateOverlayObject()
        {
            var go = new GameObject("HexOverlay");
            go.transform.SetParent(transform);

            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = _overlayMesh;

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            return go;
        }

        /// <summary>
        /// Get an overlay from the pool or create a new one.
        /// </summary>
        private GameObject GetOverlayFromPool()
        {
            if (_overlayPool.Count > 0)
            {
                return _overlayPool.Dequeue();
            }

            return CreateOverlayObject();
        }

        /// <summary>
        /// Return an overlay to the pool.
        /// </summary>
        private void ReturnOverlayToPool(GameObject overlay)
        {
            if (overlay != null)
            {
                overlay.SetActive(false);
                _overlayPool.Enqueue(overlay);
            }
        }

        private void OnTileSelected(HexTile tile)
        {
            ShowSelectionOverlay(tile);
        }

        private void OnTileDeselected(HexTile tile)
        {
            HideSelectionOverlay();
        }

        private void OnTileHovered(HexTile tile)
        {
            ShowHoverOverlay(tile);
        }

        private void OnTileHoverExit()
        {
            HideHoverOverlay();
        }

        /// <summary>
        /// Show selection overlay on a tile.
        /// </summary>
        public void ShowSelectionOverlay(HexTile tile)
        {
            if (tile == null) return;

            if (_selectionOverlay == null)
            {
                _selectionOverlay = GetOverlayFromPool();
            }

            var renderer = _selectionOverlay.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _selectionMaterial;
                renderer.enabled = true;
            }

            PositionOverlay(_selectionOverlay, tile);
            _selectionOverlay.SetActive(true);
            _pulseTime = 0f;
        }

        /// <summary>
        /// Hide the selection overlay.
        /// </summary>
        public void HideSelectionOverlay()
        {
            if (_selectionOverlay != null)
            {
                _selectionOverlay.SetActive(false);
            }
        }

        /// <summary>
        /// Show hover overlay on a tile.
        /// </summary>
        public void ShowHoverOverlay(HexTile tile)
        {
            if (tile == null) return;

            // Don't show hover on selected tile
            if (selectionManager != null && selectionManager.IsSelected(tile)) return;

            if (_hoverOverlay == null)
            {
                _hoverOverlay = GetOverlayFromPool();
            }

            var renderer = _hoverOverlay.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _hoverMaterial;
            }

            PositionOverlay(_hoverOverlay, tile);
            _hoverOverlay.SetActive(true);
        }

        /// <summary>
        /// Hide the hover overlay.
        /// </summary>
        public void HideHoverOverlay()
        {
            if (_hoverOverlay != null)
            {
                _hoverOverlay.SetActive(false);
            }
        }

        /// <summary>
        /// Position an overlay on a tile.
        /// </summary>
        private void PositionOverlay(GameObject overlay, HexTile tile)
        {
            float heightOffset = visualConfig != null ? visualConfig.overlayHeightOffset : 0.1f;

            var tileObj = HexGridManager.Instance?.GetTileObject(tile.Coordinates);
            float y = tileObj != null ? tileObj.GetTopSurfaceY() : tile.WorldPosition.y;

            overlay.transform.position = new Vector3(
                tile.WorldPosition.x,
                y + heightOffset,
                tile.WorldPosition.z
            );
        }

        /// <summary>
        /// Update the pulse animation on the selection overlay.
        /// </summary>
        private void UpdatePulseAnimation()
        {
            if (_selectionOverlay == null || visualConfig == null) return;

            float alpha = visualConfig.GetPulseAlpha(_pulseTime);
            Color color = visualConfig.selectionColor;
            color.a = alpha;

            if (_selectionMaterial != null)
            {
                _selectionMaterial.color = color;
                _selectionMaterial.SetColor("_BaseColor", color);
            }
        }

        private void OnDestroy()
        {
            // Clean up materials
            if (_selectionMaterial != null)
            {
                Destroy(_selectionMaterial);
            }

            if (_hoverMaterial != null)
            {
                Destroy(_hoverMaterial);
            }

            if (_overlayMesh != null)
            {
                Destroy(_overlayMesh);
            }
        }
    }
}
