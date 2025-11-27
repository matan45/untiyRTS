using UnityEngine;
using RTS.Terrain.Core;

namespace RTS.Terrain.Rendering
{
    /// <summary>
    /// Component attached to each hex tile GameObject.
    /// Links the visual representation to the HexTile data model.
    /// Works with both traditional rendering and GPU instancing modes.
    /// </summary>
    public class HexTileObject : MonoBehaviour
    {
        private HexTile _tileData;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private MaterialPropertyBlock _propertyBlock;

        /// <summary>
        /// The hex tile data associated with this object.
        /// </summary>
        public HexTile TileData => _tileData;

        /// <summary>
        /// The coordinates of this tile.
        /// </summary>
        public Vector2Int Coordinates => _tileData?.Coordinates ?? Vector2Int.zero;

        /// <summary>
        /// The terrain type of this tile.
        /// </summary>
        public TerrainType TerrainType => _tileData?.TerrainType ?? TerrainType.Grassland;

        /// <summary>
        /// The mesh renderer component.
        /// </summary>
        public MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                    _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }

        /// <summary>
        /// The mesh filter component.
        /// </summary>
        public MeshFilter MeshFilter
        {
            get
            {
                if (_meshFilter == null)
                    _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter;
            }
        }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Initialize this tile object with data.
        /// </summary>
        /// <param name="tile">The hex tile data to link</param>
        public void Initialize(HexTile tile)
        {
            _tileData = tile;
            gameObject.name = $"HexTile_{tile.Coordinates.x}_{tile.Coordinates.y}";
        }

        /// <summary>
        /// Get the Y position of the top surface of this tile.
        /// Used for positioning overlays and selection visuals.
        /// </summary>
        public float GetTopSurfaceY()
        {
            // Try MeshFilter first (traditional mode)
            if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                return transform.position.y + _meshFilter.sharedMesh.bounds.max.y * transform.localScale.y;
            }
            // Fall back to MeshCollider (GPU instancing mode)
            if (_meshCollider != null && _meshCollider.sharedMesh != null)
            {
                return transform.position.y + _meshCollider.sharedMesh.bounds.max.y * transform.localScale.y;
            }
            return transform.position.y;
        }

        /// <summary>
        /// Get the center position at the top surface of this tile.
        /// </summary>
        public Vector3 GetTopSurfaceCenter()
        {
            return new Vector3(
                transform.position.x,
                GetTopSurfaceY(),
                transform.position.z
            );
        }

        /// <summary>
        /// Update the visual appearance of this tile.
        /// </summary>
        /// <param name="material">The material to apply</param>
        public void RefreshVisual(Material material)
        {
            if (_meshRenderer != null && material != null)
            {
                _meshRenderer.sharedMaterial = material;
            }
        }

        /// <summary>
        /// Update the visual appearance with a color (uses property block for batching).
        /// </summary>
        /// <param name="color">The color to apply</param>
        public void RefreshVisualColor(Color color)
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_BaseColor", color);
                _propertyBlock.SetColor("_Color", color);
                _meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>
        /// Clear any property block overrides.
        /// </summary>
        public void ClearPropertyBlock()
        {
            if (_meshRenderer != null)
            {
                _propertyBlock.Clear();
                _meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>
        /// Enable or disable the mesh renderer.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = visible;
            }
        }
    }
}
