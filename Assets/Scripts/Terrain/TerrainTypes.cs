namespace RTS.Terrain
{
    /// <summary>
    /// Terrain type enumeration for categorizing different terrain tiles.
    /// </summary>
    public enum TerrainType
    {
        Grassland,
        Plains,
        Desert,
        Tundra,
        Snow,
        Hills,
        Mountains,
        Water,
        DeepWater,
        Forest,
        Swamp
    }

    /// <summary>
    /// Fog of war state for a tile.
    /// </summary>
    public enum FogOfWarState
    {
        Unexplored,  // Never seen
        Explored,    // Seen before but not currently visible
        Visible      // Currently visible
    }
}
