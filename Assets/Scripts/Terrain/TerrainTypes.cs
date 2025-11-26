namespace RTS.Terrain
{
    /// <summary>
    /// Types of terrain for hexagonal tiles.
    /// Affects buildability, movement, and visuals.
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
    /// Fog of war state for tiles.
    /// </summary>
    public enum FogOfWarState
    {
        Unexplored,
        Explored,
        Visible
    }

    /// <summary>
    /// Biome categories that group related terrain types.
    /// Used for game mechanics that apply to biome groups.
    /// </summary>
    public enum BiomeType
    {
        Temperate,    // Grassland, Plains, Forest
        Arid,         // Desert
        Arctic,       // Tundra, Snow
        Mountainous,  // Hills, Mountains
        Aquatic       // Water, DeepWater, Swamp
    }
}
